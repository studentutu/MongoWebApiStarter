﻿using Data;
using MlkPwgen;
using SCVault;
using SCVault.Auth;
using ServiceStack;
using System.Linq;

namespace Main.Account.Save
{
    [Authenticate(ApplyTo.Patch)]
    public class Service : Service<Request, Response, Data.Account>
    {
        public bool NeedsEmailVerification = false;

        public override Response Post(Request r)
        {
            return Patch(r);
        }

        [Need(Claim.AccountID)]
        public override Response Patch(Request r)
        {
            r.ID = User.ClaimValue(Claim.AccountID); //tamper protection

            CheckIfEmailValidationIsNeeded(r);

            var acc = ToEntity(r);

            if (r.ID.HasValue()) // existing account
            {
                RepoAccount.SavePreserving(acc);

                var vpID = RepoVP.Find(v => v.OwnerAccount.ID == r.ID, v => v.ID).Single();

                RepoVP.UpdateName(vpID, r.VPName);
            }
            else // new account
            {
                var vp = new VirtualPractice
                {
                    Name = r.VPName,
                };
                RepoVP.Save(vp);

                acc.VirtualPractice = vp.ID;

                RepoAccount.Save(acc);
                RepoVP.SetOwnerAccount(vp.ID, acc.ID);
            }

            SendVerificationEmail(acc);

            Response.EmailSent = NeedsEmailVerification;
            Response.ID = acc.ID;

            return Response;
        }

        protected override Data.Account ToEntity(Request r)
        {
            return new Data.Account()
            {
                ID = r.ID,
                Email = r.EmailAddress.LowerCase(),
                PasswordHash = r.Password.SaltedHash(),
                Title = r.Title,
                FirstName = r.FirstName.ToTitleCase(),
                LastName = r.LastName.ToTitleCase(),
                Address = new Address
                {
                    Street = r.Street.ToTitleCase(),
                    City = r.City,
                    State = r.State,
                    ZipCode = r.ZipCode,
                    CountryCode = r.CountryCode
                },
                TimeZoneOffsetHrs = r.TimeZoneOffsetHrs,
                Mobile = r.Mobile,
            };
        }

        private void SendVerificationEmail(Data.Account a)
        {
            if (NeedsEmailVerification)
            {
                var code = PasswordGenerator.Generate(20);
                RepoAccount.SetEmailValidationCode(code, a.ID);

                var salutation = $"{a.Title} {a.FirstName} {a.LastName}";

                var email = new SCVault.Models.Email(
                    Settings.Email.FromName,
                    Settings.Email.FromEmail,
                    salutation,
                    a.Email,
                    "Please validate your Virtual Practice account...",
                    EmailTemplates.Email_Address_Validation);

                email.MergeFields.Add("Salutation", salutation);
                email.MergeFields.Add("ValidationLink", $"{BaseURL}#/account/{a.ID}-{code}/validate");

                email.AddToSendingQueue();
            }
        }

        private void CheckIfEmailValidationIsNeeded(Request r)
        {
            if (r.ID.HasNoValue())
            {
                NeedsEmailVerification = true;
            }
            else if (r.ID != RepoAccount.GetID(r.EmailAddress.LowerCase()))
            {
                NeedsEmailVerification = true;
            }
            else
            {
                NeedsEmailVerification = false;
            }
        }
    }
}