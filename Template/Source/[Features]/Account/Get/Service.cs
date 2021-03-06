﻿using MongoWebApiStarter;
using MongoWebApiStarter.Auth;
using ServiceStack;
using System.Threading.Tasks;

namespace Account.Get
{
    public class Service : Service<Request, Response>
    {
        [
            Need(Claim.AccountID),
            Permission(Allow.Account_Read)
        ]
        public async Task<Response> GetAsync(Request r)
        {
            var acc = await Data.GetAccountAsync(r.AccountID);

            if (acc == null)
                throw HttpError.NotFound("Unable to find the right Account!");

            Response.FromEntity(acc);

            return Response;
        }
    }
}
