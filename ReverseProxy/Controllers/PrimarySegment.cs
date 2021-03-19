/**
 *
 * MIT License
 *
 * Copyright (c) 2018 Jagan Mohan Maddukuri
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 * */

namespace ReverseProxy.Controllers
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [Route("[controller]")]
    [ApiController]
    public class PrimarySegment : ControllerBase
    {
        // GET: /PrimarySegment
        [HttpGet]
        public IEnumerable<TargetService> Get()
        {
            return ReverseProxy.primarySegmentDictionary.Values;
        }

        // GET /PrimarySegment/serviceId
        [HttpGet("{serviceId}")]
        public TargetService Get(string serviceId)
        {
            TargetService targetService = null;

            if (!string.IsNullOrEmpty(serviceId))
            {
                ReverseProxy.primarySegmentDictionary.TryGetValue(serviceId, out targetService);
            }

            return targetService;
        }

        // POST /PrimarySegment
        [HttpPost]
        public string Post([FromBody] IEnumerable<TargetService> listOfServices)
        {
            int count = 0;
            foreach (TargetService service in listOfServices)
            {
                if (!string.IsNullOrEmpty(service.primarySegment))
                {
                    count++;
                    ReverseProxy.primarySegmentDictionary.AddOrUpdate(service.primarySegment, service, (serviceId, oldValue) => service);
                }
            }

            return count == listOfServices.Count() ? Constants.Success : Constants.Failure;
        }

        // PUT /PrimarySegment
        [HttpPut]
        public string Put([FromBody] TargetService targetService)
        {
            TargetService addedService = null;
            if (!string.IsNullOrEmpty(targetService.primarySegment))
            {
                addedService = ReverseProxy.primarySegmentDictionary.AddOrUpdate(targetService.primarySegment, targetService, (serviceId, oldValue) => targetService);
            }

            return addedService != null ? Constants.Success : Constants.Failure;
        }

        // DELETE /PrimarySegment/serviceId
        [HttpDelete("{serviceId}")]
        public string Delete(string serviceId)
        {
            TargetService targetService = null;
            bool isRemoved = false;
            if (!string.IsNullOrEmpty(serviceId))
            {
                isRemoved = ReverseProxy.primarySegmentDictionary.TryRemove(serviceId, out targetService);
            }

            return isRemoved? Constants.Success : Constants.Failure;
        }
    }
}
