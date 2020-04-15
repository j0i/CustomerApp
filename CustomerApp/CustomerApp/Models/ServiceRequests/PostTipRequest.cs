﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CustomerApp.Models.ServiceRequests
{
    class PostTipRequest : ServiceRequest
    {
        //the endpoint we are trying to hit
        public override string Url => "https://dijkstras-steakhouse-restapi.herokuapp.com/tips/";
        //the type of request
        public override HttpMethod Method => HttpMethod.Post;
        //headers if we ever need them
        public override Dictionary<string, string> Headers => null;

        public TipObject Body;

        // Constructor containing the employee ID and tip amount to be sent
        PostTipRequest(string ID, double amt)
        {
            TipObject t = new TipObject();

            t.employee_id = ID;
            t.tip_amount = amt;

            Body = t;
        }

        // Request body content object
        public class TipObject
        {
            public string employee_id;
            public double tip_amount;
        }


        // Posts a tip to the database
        public static async Task<bool> SendPostTipRequest(string ID, double amt)
        {
            //make a new request object
            var serviceRequest = new PostTipRequest(ID, amt);
            //get a response
            var response = await ServiceRequestHandler.MakeServiceCall<DeleteResponse>(serviceRequest, serviceRequest.Body);

            if(response == null)
            {
                //call failed
                return false;
            }
            else
            {
                //call succeeded
                return true;
            }
        }
    }
}
