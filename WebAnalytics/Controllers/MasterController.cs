using Domain.Constants;
using Domain.interfaces;
using Domain.Requests;
using Domain.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAnalytics.Controllers
{
    public class MasterController : Controller
    {
        private readonly IUnitOfWork unitOfWork; 
        public MasterController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        #region Master Table 
        [AllowAnonymous]
        public async Task<IActionResult> GetAllMMaster(DTOMasterRequest Data)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var ret = await unitOfWork.GetAllMMaster(Data);
                var response = new DTOGenericResponse<object>(ConnKeyConstants.Success, ConnKeyConstants.SuccessMessage, ret);
                return Json(response);
            }
            catch
            {
                var response = new DTOGenericResponse<object>(ConnKeyConstants.InternalServerError, ConnKeyConstants.InternalServerErrorMessage, "Null");
                return Json(response);
            }
        }
#endregion Master Table
    }
}
