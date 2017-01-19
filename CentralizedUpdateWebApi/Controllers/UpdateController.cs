using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Routing;
using UpdateExecutableCommon.DataModels;
using CentralizedUpdateWebApi.Utilities;

namespace CentralizedUpdateWebApi.Controllers
{
    //[RoutePrefix("update")]
    public class UpdateController : ApiController
    {
        private UpdateRepository _UpdateRepo = null;

        public UpdateController()
        {
            _UpdateRepo = new UpdateRepository();
        }

        //POST: api/update/postclientinfo
        [HttpPost]
        public IHttpActionResult PostClientInfo(ClientInfo clientInfo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            UpdateInfo info = _UpdateRepo.GetUpdateInfo(clientInfo, HttpContext.Current.Request.Url.ToString());

            return Ok(info);            
        }

        //POST: api/update/postupdateresults
        [HttpPost]
        public HttpResponseMessage PostUpdateStartInfo(UpdateStartInfo info)
        {
            _UpdateRepo.SaveUpdateStartInfo(info);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        //POST: api/update/postupdateresults
        [HttpPost]
        public HttpResponseMessage PostUpdateResults(UpdateResults results)
        {
            _UpdateRepo.SaveUpdateResults(results);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        //GET: api/update/getresource?key={key}
        public HttpResponseMessage GetResource(string key)
        {
            byte[] zip = _UpdateRepo.GetUpdateResource(key);

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new ByteArrayContent(zip);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            return result;
        }
    }
}
