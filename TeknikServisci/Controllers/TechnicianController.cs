﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Microsoft.AspNet.Identity;
using TeknikServisci.BLL.Repository;
using TeknikServisci.Models.Enums;
using TeknikServisci.Models.IdentityModels;
using TeknikServisci.Models.ViewModels;
using static TeknikServisci.BLL.Identity.MembershipTools;


namespace TeknikServisci.Controllers
{
    public class TechnicianController : Controller
    {
        List<SelectListItem> Technicians = new List<SelectListItem>();
        // GET: Technician
        public ActionResult Index()
        {
            try
            {
                var technicianId = HttpContext.User.Identity.GetUserId();
                var data = new FailureRepo()
                    .GetAll(x => x.TechnicianId == technicianId).Select(x => Mapper.Map<FailureViewModel>(x))
                    .ToList();

                return View(data);
            }
            catch (Exception ex)
            {
                TempData["Model"] = new ErrorViewModel()
                {
                    Text = $"Bir hata oluştu {ex.Message}",
                    ActionName = "Index",
                    ControllerName = "Home",
                    ErrorCode = 500
                };
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult FailureList()
        {
            var techId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            try
            {
                var data = new FailureRepo()
                    .GetAll()
                    .Select(x => Mapper.Map<FailureViewModel>(x))
                    .Where(x=>x.TechnicianId == techId)
                    .OrderBy(x => x.OperationTime)
                    .ToList();
                return View(data);
            }
            catch (Exception ex)
            {
                TempData["Model"] = new ErrorViewModel()
                {
                    Text = $"Bir hata oluştu {ex.Message}",
                    ActionName = "Index",
                    ControllerName = "Home",
                    ErrorCode = 500
                };
                return RedirectToAction("Error", "Home");

            }

        }

        public async Task<ActionResult> Detail(int id)
        {
            try
            {
                var x = await new FailureRepo().GetByIdAsync(id);
                var data = Mapper.Map<FailureViewModel>(x);

                var TechnicianRole = NewRoleManager().FindByName("Technician").Users.Select(y => y.UserId).ToList();
                for (int i = 0; i < TechnicianRole.Count; i++)
                {

                    var User = NewUserManager().FindById(TechnicianRole[i]);
                    Technicians.Add(new SelectListItem()
                    {
                        Text = User.Name + " " + User.Surname,
                        Value = User.Id
                    });
                }

                ViewBag.TechnicianList = Technicians;


                return View(data);


            }
            catch (Exception ex)
            {
                TempData["Model"] = new ErrorViewModel()
                {
                    Text = $"Bir hata oluştu {ex.Message}",
                    ActionName = "Detail",
                    ControllerName = "Operator",
                    ErrorCode = 500
                };
                return RedirectToAction("Error", "Home");
            }

        }

        [HttpPost]
        public async Task<ActionResult> TechnicianStartWork(FailureViewModel model)
        {
            try
            {
                var failure = await new FailureRepo().GetByIdAsync(model.FailureId);

                switch (failure.Technician.TechnicianStatus)
                {
                    case TechnicianStatuses.Available:
                        model.TechnicianStatus = TechnicianStatuses.OnWay;
                        //todo: Kullanıcıya mail gitsin.
                        break;
                    case TechnicianStatuses.OnWay:
                        model.TechnicianStatus = TechnicianStatuses.OnWork;
                        break;
                    case TechnicianStatuses.OnWork:
                        model.TechnicianStatus = TechnicianStatuses.Available;
                        //todo: Rapor sayfasına yönlendirsin. Rapor sayfasında açıklama, garanti bilgisi ve fiyat girilsin.
                        break;
                    default:
                        break;
                }

                failure.Report = model.Report;
                failure.Technician.TechnicianStatus = model.TechnicianStatus;
                if (model.RepairProcess == RepairProcesses.Successful)
                {
                    failure.FinishingTime = DateTime.Now;
                }
                else if(model.RepairProcess == RepairProcesses.Failed)
                    failure.FinishingTime = DateTime.Now;
             

                new FailureRepo().Update(failure);
                TempData["Message"] = $"{model.FailureId} no lu arıza için yola çıkılmıştır";
                return RedirectToAction("Detail", "Technician",new
                {
                    id = model.FailureId
                });

            }
            catch (Exception ex)
            {
                TempData["Model"] = new ErrorViewModel()
                {
                    Text = $"Bir hata oluştu {ex.Message}",
                    ActionName = "TechnicianStartWork",
                    ControllerName = "Technician",
                    ErrorCode = 500
                };
                return RedirectToAction("Error", "Home");
            }

        }
        public ActionResult TechnicianReport(int id)
        {
            try
            {
                var failure = new FailureRepo().GetById(id);
                var data = Mapper.Map<FailureViewModel>(failure);
                return View(data);
            }
            catch (Exception ex)
            {
                TempData["Model"] = new ErrorViewModel()
                {
                    Text = $"Bir hata oluştu {ex.Message}",
                    ActionName = "TechnicianReport",
                    ControllerName = "Technician",
                    ErrorCode = 500
                };
                return RedirectToAction("Error", "Home");

            }

        }

        public async Task<ActionResult> CreateInvoice(int id)
        {
            try
            {
                var x = await new FailureRepo().GetByIdAsync(id);
                var data = Mapper.Map<FailureViewModel>(x);

                var TechnicianRole = NewRoleManager().FindByName("Technician").Users.Select(y => y.UserId).ToList();
                for (int i = 0; i < TechnicianRole.Count; i++)
                {

                    var User = NewUserManager().FindById(TechnicianRole[i]);
                    Technicians.Add(new SelectListItem()
                    {
                        Text = User.Name + " " + User.Surname,
                        Value = User.Id
                    });
                }

                ViewBag.TechnicianList = Technicians;


                return View(data);


            }
            catch (Exception ex)
            {
                TempData["Model"] = new ErrorViewModel()
                {
                    Text = $"Bir hata oluştu {ex.Message}",
                    ActionName = "Detail",
                    ControllerName = "Operator",
                    ErrorCode = 500
                };
                return RedirectToAction("Error", "Home");
            }
        }

        //public ActionResult ChangeStatus()
        //{
        //    var user = new User();
        //    user = NewUserManager().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());

        //    if (user.TechnicianStatus == TechnicianStatuses.Available)
        //    {
        //        user.TechnicianStatus = TechnicianStatuses.Busy;
                
        //        NewUserManager().Update(user);
        //    }
        //    else
        //    {
        //        user.TechnicianStatus = TechnicianStatuses.Available;
        //        NewUserManager().Update(user);
        //    }

        //    return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
        //}
    }
}