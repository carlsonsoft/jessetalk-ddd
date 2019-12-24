using System;

namespace User.Api.Models
{
    public class UserTag
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        ///
        /// </summary>
        public string Tag { get; set; }
        public DateTime CreateDate { get; set; }
    }
}