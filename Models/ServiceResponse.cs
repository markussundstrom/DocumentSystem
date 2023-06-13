using System;
using System.Net;

namespace DocumentSystem.Models
{
    ///<summary>
    ///Class <c>ServiceResponse<T></c> is used as return value
    ///from the service layer.
    ///</summary>
    public class ServiceResponse<T> {
        public bool Success {get; set;}
        public HttpStatusCode StatusCode {get; set;}
        public T? Data {get; set;}
        public String? ErrorMessage {get; set;}
    }
}
