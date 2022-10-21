using AutoMapper;
using LeaveManagement.Application.Contracts;
using LeaveManagement.Data;
using LeaveManagement.Common.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace LeaveManagement.Application.Contracts
{
    public class LeaveRequestRepository :GenericRepository<LeaveRequest>, ILeaveRequestRepository
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IEmailSender emailSender;
        private readonly UserManager<Employee> userManager;
        private readonly ILeaveAllocationRepository leaveAllocationRepository;

        public LeaveRequestRepository(ApplicationDbContext context, 
            IMapper mapper, 
            ILeaveAllocationRepository leaveAllocationRepository,
            IHttpContextAccessor httpContextAccessor,
            IEmailSender emailSender,
            UserManager<Employee> userManager):base(context)
        {
            this.context = context;
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this.emailSender = emailSender;
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.leaveAllocationRepository = leaveAllocationRepository ?? throw new ArgumentNullException(nameof(leaveAllocationRepository));
        }

        public async Task ChangeApprovalStatus(int leaveRequestId, bool approved)
        {
            var leaveRequest = await GetAsync(leaveRequestId);
            leaveRequest.Approved = approved;
            

            if (approved)
            {
                var allocation= await leaveAllocationRepository.GetEmployeeAllocation(leaveRequest.RequestingEmployeeId, leaveRequest?.LeaveTypeId);
                int daysRequested = (int)(leaveRequest.EndDate - leaveRequest.StartDate).TotalDays;
                allocation.NumberOfDays = daysRequested;

              await leaveAllocationRepository.UpdateAsync(allocation);

            }
           await UpdateAsync(leaveRequest);

            var user = await userManager.FindByIdAsync(leaveRequest.RequestingEmployeeId);
            var approvalStatus = approved ? "Approved" : "Decline";
            await emailSender.SendEmailAsync(user.Email, $"Leave Request {approvalStatus}",
               $"Your leave request from " + $"{leaveRequest.StartDate} to {leaveRequest.EndDate} has been  {approvalStatus}");

        }

        public async Task<bool> CreateLeaveRequest(LeaveRequestCreateVM model)
        {
            var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext?.User);

            var leaveAllocation = await leaveAllocationRepository.GetEmployeeAllocation(user.Id,
                model.LeaveTypeId);

            if(leaveAllocation==null)
            {
                return false;
            }
            int daysRequested = (int)(model.EndDate - model.StartDate).TotalDays;

            if(daysRequested>leaveAllocation.NumberOfDays)
            {
                return false;
            }

            var leaveRequest = mapper.Map<LeaveRequest>(model);
            leaveRequest.DateRequested = DateTime.Now;
            leaveRequest.RequestingEmployeeId = user.Id;

            await AddAsync(leaveRequest);

            await emailSender.SendEmailAsync(user.Email, "Leave Request Submittrd Seccessfully", 
                $"Your leave request from "+ $"{leaveRequest.StartDate} to {leaveRequest.EndDate} has been submitted for approval");

            return true;
        }

        public async Task<AdminLeaveRequestViewVM> GetAdminLeaveRequestList()
        {
            var leaveRequests= await context.LeaveRequests.Include(q=>q.LeaveType).ToListAsync();
            var model = new AdminLeaveRequestViewVM
            {
                TotalRequests = leaveRequests.Count,
                ApprovedRequests=leaveRequests.Count(q=> q.Approved==true),
                PendingRequests=leaveRequests.Count(q=> q.Approved==null),
                RejectedRequests=leaveRequests.Count(q=> q.Approved==false),
                LeaveRequests= mapper.Map<List<LeaveRequestVM>>(leaveRequests),
            };

            foreach(var leaveRequest in model.LeaveRequests)
            {
                leaveRequest.Employee = mapper.Map<EmployeeListVM>(await userManager.FindByIdAsync(leaveRequest.RequestingEmployeeId));
            }


            return model;   
        }

        public async Task<List<LeaveRequest>> GetAllAsync(string employeeId)
        {
            return await context.LeaveRequests.Where(q=> q.RequestingEmployeeId==employeeId).ToListAsync();
        }

        public async Task<LeaveRequestVM?> GetLeaveRequestAsync(int? id)
        {
            var leaveRequest = await context.LeaveRequests
              .Include(q => q.LeaveType)
              .FirstOrDefaultAsync(q => q.Id == id);

            if (leaveRequest == null)
            {
                return null;
            }

            var model = mapper.Map<LeaveRequestVM>(leaveRequest);
            model.Employee = mapper.Map<EmployeeListVM>(await userManager.FindByIdAsync(leaveRequest?.RequestingEmployeeId));
            return model;

        }
         
        public async Task<EmployeeLeaveRequestViewVM> GetMyLeaveDetails()
        {
            var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext?.User);
            var allocations = (await leaveAllocationRepository.GetEmployeeAllocations(user.Id)).LeaveAllocations; 
            var requests= mapper.Map<List<LeaveRequestVM>>(await GetAllAsync(user.Id));
            var model = new EmployeeLeaveRequestViewVM(allocations, requests);
            return model;
        }
    }
}
