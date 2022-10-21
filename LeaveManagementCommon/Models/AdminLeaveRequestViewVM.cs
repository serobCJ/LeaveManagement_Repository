using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.Common.Models
{
    public class AdminLeaveRequestViewVM
    {
        [Display(Name = "Total Number Of Requests")]
        public int TotalRequests { set; get; }

        [Display(Name = "Approved Requests")]
        public int ApprovedRequests { set; get; }

        [Display(Name = "Pending Requests")]
        public int PendingRequests { set; get; }

        [Display(Name = "Rejected Requests")]
        public int RejectedRequests { set; get; }

        public List<LeaveRequestVM> LeaveRequests { get; set; }   
    }
}
