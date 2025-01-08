using AI_Chatbot.DTOs;
using AI_Chatbot.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;

namespace AI_Chatbot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IRegisterService registerService;
        private readonly IOtpService otpService;
        private readonly ILoginService loginService;

        public UserController(IRegisterService registerService,IOtpService otpService, ILoginService loginService)
        {
            this.registerService = registerService;
            this.otpService = otpService;
            this.loginService = loginService;
        }

        //[HttpPost("register")]
        //public async Task<IActionResult> Register([FromBody] RegiserDto regiser)
        //{
        //    var message = await registerService.Register(regiser);
        //    return Ok(message);
        //}

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            var user = await loginService.CheckUser(login);
            if (!user)
            {
                await registerService.Register(login);
                await Task.Delay(1000);
            }
            var otp = otpService.GenerateOtp();
            await otpService.StoreOtp(login, otp);
            await otpService.SendOtpViaMail(login.Email, "OTP for Login", $"Your OTP is {otp}");
            return Ok("OTP sent to your email...");
        }

        [HttpPost("verify")]
        public async Task<IActionResult> Verify([FromBody] OtpDto otp)
        {
            var result = await otpService.CheckOtp(otp);
            if (result == null)
            {
                return BadRequest("Invalid OTP...");
            }
            return Ok(result);
        }
    }
}
