using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Wordprocessing;
using Fujitsu_eSignPO.Data;
using Fujitsu_eSignPO.interfaces;
using Fujitsu_eSignPO.Models;
using Fujitsu_eSignPO.Services.Customer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Ocsp;
using System.ComponentModel.DataAnnotations;

namespace Fujitsu_eSignPO.Controllers
{
    public class UserManageController : Controller
    {
        private readonly FgdtESignPoContext _eSignPrpoContext;
        private readonly IAccountService _accountSv;
        private readonly IPRPOService _pRPOService;
        public UserManageController(FgdtESignPoContext eSignPoContext, IAccountService accountSv, IPRPOService pRPOService )
        {
            _eSignPrpoContext = eSignPoContext;
            _accountSv = accountSv;
            _pRPOService = pRPOService;

        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> getUser()
        {
            var response = await _eSignPrpoContext.TbEmployees.ToListAsync();
            return Json(new { data = response });
        }
        public async Task<IActionResult> InsertUpdate(string userId = null)
        {
            var response = new UserInsertUpdateModel();

            var getUserById = await _eSignPrpoContext.TbEmployees.Where(x => x.NEmpId == userId).FirstOrDefaultAsync();

            if (getUserById == null)
            {
                response.bActive = true;
                response.bSendMail = true;
                return View(response);
            }

            response = new UserInsertUpdateModel
            {
                sEmpName = getUserById?.SEmpName,
                sEmpUsername = getUserById?.SEmpUsername,
                sEmpPassword = getUserById?.SEmpPassword,
                sEmpEmail = getUserById?.SEmpEmail,
                sEmpTitle = getUserById?.SEmpTitle,
                sDepartment = getUserById?.SDepartment,
                sPosition = getUserById?.SPosition,
                nPositionLevel = getUserById?.NPositionLevel.ToString(),
                bActive = getUserById.BActive.Value,
                bSendMail = getUserById.BSendMail.Value,
                Telephone = getUserById?.Telephone,
                nEmpId = getUserById?.NEmpId
            };

            return View(response);
        }

        [HttpPost]
        public async Task<IActionResult> InsertUpdate(UserInsertUpdateModel Request, string isEdit)
        {
            var response = new JsonResponse();

            if (isEdit == "1")
            {
                response = await updateUser(Request);
            }
            else
            {
                response = await insertUser(Request);
            }

            if (!response.status)
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        public async Task<JsonResponse> insertUser(UserInsertUpdateModel Request)
        {
            var response = new JsonResponse();
            var info = _accountSv.informationUser();
            try
            {
                var insertUser = new TbEmployee
                {
                    NEmpId = generateEmpID(),
                    SEmpName = Request?.sEmpName,
                    SEmpUsername = Request?.sEmpUsername,
                    SEmpPassword = Request?.sEmpPassword,
                    SEmpEmail = Request?.sEmpEmail,
                    SEmpTitle = Request?.sEmpTitle,
                    SDepartment = Request?.sDepartment,
                    SPosition = Request?.sPosition,
                    NPositionLevel = Convert.ToInt32(Request?.nPositionLevel),
                    BActive = Request.bActive,
                    BSendMail = Request.bSendMail,
                    Telephone = Request?.Telephone,
                    DCreated = DateTime.Now,
                    SCreatedBy = info.sID


                };

                _eSignPrpoContext.TbEmployees.Add(insertUser);

                var state = await _eSignPrpoContext.SaveChangesAsync() > 0;

                response.status = state;
                response.message = "Successfully added user data to the database.";

            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = ex.InnerException.Message;

            }

            return response;
        }


        public async Task<JsonResponse> updateUser(UserInsertUpdateModel Request)
        {
            var response = new JsonResponse();
            var info = _accountSv.informationUser();


            try
            {
                var getUser = await _eSignPrpoContext.TbEmployees.Where(x => x.NEmpId == Request.nEmpId)
.FirstOrDefaultAsync();

                if (getUser != null)
                {
                    getUser.SEmpName = Request?.sEmpName;
                    getUser.SEmpUsername = Request?.sEmpUsername;
                    getUser.SEmpPassword = Request?.sEmpPassword;
                    getUser.SEmpEmail = Request?.sEmpEmail;
                    getUser.SEmpTitle = Request?.sEmpTitle;
                    getUser.SDepartment = Request?.sDepartment;
                    getUser.SPosition = Request?.sPosition;
                    getUser.NPositionLevel = Convert.ToInt32(Request?.nPositionLevel);
                    getUser.BActive = Request.bActive;
                    getUser.BSendMail = Request.bSendMail;
                    getUser.Telephone = Request?.Telephone;
                    getUser.DUpdated = DateTime.Now;
                    getUser.SUpdatedBy = info.sID;

                }


                var state = await _eSignPrpoContext.SaveChangesAsync() > 0;

                response.status = state;
                response.message = "Successfully updated user data to the database.";

            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = ex.InnerException.Message;

            }

            return response;
        }

        public string generateEmpID()
        {

            string empId = "E0001";
            var getEmpId = _eSignPrpoContext.TbEmployees.OrderByDescending(x => x.NEmpId).FirstOrDefault();

            if (getEmpId != null)
            {
                string lastIdNumber = getEmpId.NEmpId.Substring(1); // เช่น "E0005" → "0005"
                int newIdNumber = int.Parse(lastIdNumber) + 1;

                // สร้างรหัสใหม่โดยเติม 0 ด้านหน้าให้ครบ 4 หลัก
                empId = "E" + newIdNumber.ToString("D4"); // "E0006"
            }

            return empId;
        }

        public async Task<IActionResult> deleteUser(string data)
        {

            var response = new JsonResponse();
            try
            {

                var getUser = await _eSignPrpoContext.TbEmployees.Where(x => x.NEmpId == data).FirstOrDefaultAsync();

                if (getUser != null)
                {
                    _eSignPrpoContext.TbEmployees.Remove(getUser);
                }

                var state = await _eSignPrpoContext.SaveChangesAsync() > 0;

                response.status = state;
                response.message = "Successfully deleted user data to the database.";

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = ex.InnerException.Message;
                return BadRequest(response);
            }
        }



        public class UserInsertUpdateModel
        {
            public string nEmpId { get; set; }
            [Required(ErrorMessage = "Name is required.")]
            public string sEmpName { get; set; }

            [Required(ErrorMessage = "Email is required.")]
            [EmailAddress(ErrorMessage = "Invalid email format.")]
            public string sEmpEmail { get; set; }

            [Required(ErrorMessage = "Username is required.")]
            public string sEmpUsername { get; set; }

            [Required(ErrorMessage = "Password is required.")]
            public string sEmpPassword { get; set; }
            [Required(ErrorMessage = "Title is required.")]
            public string sEmpTitle { get; set; }
            [Required(ErrorMessage = "Department is required.")]
            public string sDepartment { get; set; }
            [Required(ErrorMessage = "Position is required.")]
            public string sPosition { get; set; }

            [Required(ErrorMessage = "Position Level is required.")]
            public string nPositionLevel { get; set; }
            public bool bActive { get; set; }
            public string Telephone { get; set; }
            public bool bSendMail { get; set; }
        }
    }
}
