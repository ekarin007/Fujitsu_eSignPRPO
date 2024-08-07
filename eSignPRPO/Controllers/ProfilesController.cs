using Azure;
using eSignPRPO.interfaces;
using eSignPRPO.Models.Account;
using eSignPRPO.Models.Login;
using eSignPRPO.Models.Profiles;
using eSignPRPO.Services.PRPO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using MimeKit;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Ocsp;

namespace eSignPRPO.Controllers
{
    [Authorize]
    public class ProfilesController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IProfilesService _profilesService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfilesController(IAccountService accountService, IProfilesService profilesService, IWebHostEnvironment webHostEnvironment)
        {
            _accountService = accountService;
            _profilesService = profilesService;
            _webHostEnvironment = webHostEnvironment;

        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ResetPassword()
        {
            var response = new ResetPasswordModel();

            var getInfo = _accountService.informationUser();

            response = new ResetPasswordModel
            {
                userName = getInfo?.sID
            };
            return View(response);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel request)
        {
            var response = new Tuple<bool, string>(false, "Please check old password.");

            if (!request.newPassword.Equals(request.confirmPassword))
            {
                return NotFound(new { status = response.Item1, msg = "New password and Confirm Password dosen't match , Please Try Again." });
            }
            var credential = new Credential
            {
                UserName = request?.userName,
                Password = request?.oldPassword
            };
            var checkSupLogin = await _accountService.checkSupplierLogin(credential);

            if (checkSupLogin != null)
            {
                response = await _profilesService.resetPaswordSupplier(request);
            }

            var checkEmpLogin = await _accountService.checkLoginUser(credential);

            if (checkEmpLogin != null)
            {
                response = await _profilesService.resetPaswordEmp(request);
            }

            if (!response.Item1)
            {
                return NotFound(new { status = response.Item1, msg = response.Item2 });
            }
            return Ok(new { status = response.Item1, msg = response.Item2 });

        }
        [Authorize(Roles = "5")]
        public async Task<IActionResult> uploadSignature()
        {
            var res = new UploadFileModel();
            var info = _accountService.informationUser();

            res.empId = info.sID;

            var getEmpData = await _profilesService.getEmpByID(res.empId);

            if (getEmpData.SSignature != null)
            {
                res.fileUploads = new List<Models.PRPO.fileUpload>
                {
                new Models.PRPO.fileUpload
                    {
                        sAttach_Name = getEmpData?.SSignature
                    }
                };
            }

            return View(res);
        }

        [HttpPost]
        public async Task<IActionResult> UploadFiles(List<IFormFile> files, string empId)
        {
            try
            {

                string pathFile = $"{this._webHostEnvironment.WebRootPath}\\signature\\";
               
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        
                        var filePath = Path.Combine(pathFile, empId + ".png");
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                    }
                }

                var updateSignature = await _profilesService.updateSignature(files, empId);

                if (updateSignature)
                {
                    var signature = await _profilesService.getSignature(empId);
                    return Ok(new { msg = "Files uploaded successfully.", signature });
                }
                else
                {
                    return NotFound("Files uploaded failed.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex?.Message);
            }
        }

        public IActionResult ViewFile(string fileName)
        {
            string pathFile = $"{this._webHostEnvironment.WebRootPath}\\signature\\";
            var filePath = Path.Combine(pathFile, fileName);

            if (System.IO.File.Exists(filePath))
            {

                var fileContent = System.IO.File.ReadAllBytes(filePath);

                var contentType = MimeTypes.GetMimeType(fileName);

                return File(fileContent, contentType);
            }

            // If the file doesn't exist, you can handle the error and return an appropriate response
            return NotFound();
        }

        public async Task<IActionResult> DeleteFile(string fileName, string empId)
        {

            string pathFile = $"{this._webHostEnvironment.WebRootPath}\\signature\\";

            var filePath = Path.Combine(pathFile, empId + ".png");

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);

                var delFile = await _profilesService.DeleteFile(fileName, empId);
                if (!delFile)
                {
                    return NotFound("File deleted failed.");
                }

                var attList = await _profilesService.getSignature(empId);

                return Ok(new { msg = "File deleted successfully.", attList });

            }
            else
            {
                return NotFound("File not found.");
            }
        }


    }
}
