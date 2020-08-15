using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRUDExample.Models
{
    public class Response<TData>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public TData Data { get; set; }
        //public string ErrorID { get; set; }
    }

}