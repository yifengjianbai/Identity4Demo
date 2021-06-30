using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using WebApi.Models.Response;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize]
    public class LoginController : Controller
    {
        public IConfiguration _Configuration;
        public LoginController(IConfiguration configuration)
        {
            _Configuration = configuration;
        }
        
        /// <summary>
        /// 登录
        /// </summary>
        /// <returns>登录成功，返回token</returns>
        [AllowAnonymous]
        public async Task<ResponseBase<string>> Login()
        {
            var response = new ResponseBase<string>();
            //验证用户密码
            bool res = true;
            if (res)
            {
                var client = new HttpClient();
                string OAuthAddress = ((ConfigurationSection)_Configuration.GetSection("AppSetting:IdentityServerUrl")).Value;
                string ClientId = ((ConfigurationSection)_Configuration.GetSection("AppSetting:OAuthClientId")).Value;
                string Scope = ((ConfigurationSection)_Configuration.GetSection("AppSetting:Scope")).Value;
                string ClientSecret = ((ConfigurationSection)_Configuration.GetSection("AppSetting:ClientSecret")).Value;

                //鉴权、授权中心
                var discovery = await client.GetDiscoveryDocumentAsync(OAuthAddress);
                if (discovery.IsError)
                {
                    response.Code = 401;
                    response.Message = "授权验证失败";
                    return response;
                }

                // request token
                var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
                {
                    Address = discovery.TokenEndpoint,
                    ClientId = ClientId,
                    ClientSecret = ClientSecret,
                    Scope = Scope
                });

                if (tokenResponse.IsError)
                {
                    response.Code = 401;
                    response.Message = "授权验证失败";
                    return response;
                }
                response.Result = tokenResponse.AccessToken;
            }
            else
            {
                response.Code = 402;
                response.Message = "用户名或密码错误";
            }
            return response;
        }
    }
}