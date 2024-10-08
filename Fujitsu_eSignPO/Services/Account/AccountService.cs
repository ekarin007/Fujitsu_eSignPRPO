﻿using Fujitsu_eSignPO.Data;
using Fujitsu_eSignPO.interfaces;
using Fujitsu_eSignPO.Models;
using Fujitsu_eSignPO.Models.Account;
using Fujitsu_eSignPO.Models.Login;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Fujitsu_eSignPO.Services.Account
{
    public class AccountService : IAccountService
    {
        private static FgdtESignPoContext _eSignPrpoContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AccountService(FgdtESignPoContext eSignPrpoContext, IHttpContextAccessor httpContextAccessor)
        {
            _eSignPrpoContext = eSignPrpoContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<TbEmployee> checkLoginUser(Credential credential) => await _eSignPrpoContext.TbEmployees.Where(x => x.SEmpUsername == credential.UserName && x.SEmpPassword == credential.Password && x.BActive == true).FirstOrDefaultAsync();

        public async Task<TbCustomer> checkSupplierLogin(Credential credential) => await _eSignPrpoContext.TbCustomers.Where(x => x.SCusUsername == credential.UserName && x.SCusPassword == credential.Password && x.BActive == true).FirstOrDefaultAsync();
        public informationData informationUser()
        {
            var context = _httpContextAccessor.HttpContext;
            var claim = context.User.Claims;
            var informationData = new informationData
            {
                sID = claim.FirstOrDefault(x => x.Type == ClaimTypes.Sid)?.Value,
                name = claim.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value,
                department = claim.FirstOrDefault(x => x.Type == "Department")?.Value,
                position = claim.FirstOrDefault(x => x.Type == "Position")?.Value,
                positionLevel = claim.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value,
                title = claim.FirstOrDefault(x => x.Type == "Title")?.Value
            };
            return informationData;
        }

       
    }
}
