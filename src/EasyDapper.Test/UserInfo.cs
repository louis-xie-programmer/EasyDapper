using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EasyDapper.Test
{
    /// <summary>
    /// 用户信息
    /// </summary>
    public partial class UserInfo
    {

        /// <summary>
        /// Int32:
        /// </summary>
        [Key]
        public int UserID { get; set; }


        /// <summary>
        /// String:用户邮箱，用户名
        /// </summary>
        [StringLength(50)]
        public string Email { get; set; }


        /// <summary>
        /// String:
        /// </summary>
        [StringLength(20)]
        public string Password { get; set; }


        /// <summary>
        /// Guid:
        /// </summary>
        public Guid? CompanyGuid { get; set; }


        /// <summary>
        /// String:
        /// </summary>
        [StringLength(25)]
        public string? CompanyName { get; set; }


        /// <summary>
        /// Int32:
        /// </summary>
        public int? CreatedClientInfoID { get; set; }


        /// <summary>
        /// Int32:
        /// </summary>
        [DefaultValue(0)]
        public int? DistributorID { get; set; }


        /// <summary>
        /// Boolean:
        /// </summary>
        [DefaultValue(false)]
        public bool IsAdmin { get; set; }


        /// <summary>
        /// DateTime:
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// DateTime:
        /// </summary>
        public DateTime? UpdateTime { get; set; }

    }
}
