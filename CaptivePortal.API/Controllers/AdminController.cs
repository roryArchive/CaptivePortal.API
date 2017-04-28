﻿using CaptivePortal.API.Context;
using CaptivePortal.API.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Text.RegularExpressions;

namespace CaptivePortal.API.Controllers
{
    public class AdminController : Controller
    {
        CPDBContext db = new CPDBContext();
        string ConnectionString = ConfigurationManager.ConnectionStrings["CPDBContext"].ConnectionString;
        //int orgId = 0;
        //int compId = 0;
        //int siteId = 0;
        //int formId = 0;
        //string imagepath = null;
        //Form objForm = new Form();


        /// <summary>
        /// login operation for global admin.
        /// </summary>
        /// <param name="admin"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GAlogin")]
        public ActionResult GALogin(AdminLoginViewModel admin)
        {
            try
            {
                string retString = "-1";
                if (!string.IsNullOrEmpty(admin.UserName) && !string.IsNullOrEmpty(admin.Password))
                {
                    Users user = db.Users.Where(m => m.UserName == admin.UserName).FirstOrDefault();
                    if (user != null)
                    {
                        retString = Convert.ToString(user);
                    }
                }
                else
                {
                    return RedirectToAction("Login", "Admin");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return RedirectToAction("Index", "Admin");
        }

        // GET: Global Admin
        public ActionResult Login()
        {
            return View();
        }
        //Get:Admin will create user.
        public ActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Populate company list in dropdown.
        /// </summary>
        /// <returns>Company details</returns>
        ///Get:Create new site 
        public ActionResult CreateNewSite()
        {
            ViewBag.companies = from item in db.Company.ToList()
                                select new SelectListItem()
                                {
                                    Text = item.CompanyName,
                                    Value = item.CompanyId.ToString(),
                                };
            return View();
        }

        /// <summary>
        /// papulate organisation list in dropdown on select of company.
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public JsonResult GetOrganisations(int companyId)
        {
            var result = from item in db.Company.Where(m => m.CompanyId == companyId).ToList()
                         select new
                         {
                             value = item.Organisation.OrganisationId,
                             text = item.Organisation.OrganisationName
                         };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Create new site/org/comp/field.
        /// </summary>
        /// <param name="inputData"></param>
        /// <param name="fc"></param>
        /// <param name="dataType"></param>
        /// <param name="controlType"></param>
        /// <param name="fieldLabel"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreateSiteAndLoginRegisterConf(FormViewModel inputData, FormCollection fc, string[] dataType, string[] controlType, string[] fieldLabel)
        {
            try
            {

                string imagepath = null;
                int orgId = inputData.organisationDdl;
                int compId = inputData.CompanyDdl;

                //organisation
                if (inputData.OrganisationName != null)
                {
                    Organisation objOrganisation = new Organisation
                    {
                        OrganisationName = inputData.OrganisationName
                    };
                    db.Organisation.Add(objOrganisation);
                    db.SaveChanges();
                    orgId = objOrganisation.OrganisationId;
                }
                //company
                if (inputData.CompanyName != null)
                {
                    Company objCompany = new Company
                    {
                        CompanyName = inputData.CompanyName,
                        OrganisationId = orgId,
                    };
                    db.Company.Add(objCompany);
                    db.SaveChanges();
                    compId = objCompany.CompanyId;
                }

                //site
                Site objSite = new Site
                {
                    SiteName = inputData.SiteName,
                    CompanyId = compId,
                    AutoLogin=inputData.AutoLogin
                };
                db.Site.Add(objSite);
                db.SaveChanges();

                //image path
                if (Request.Files["BannerIcon"].ContentLength > 0)
                {
                    var httpPostedFile = Request.Files["BannerIcon"];
                    string savedPath = HostingEnvironment.MapPath("/Images/" + objSite.SiteId);
                    imagepath = "/Images/" + objSite.SiteId + "/" + httpPostedFile.FileName;
                    string completePath = Path.Combine(savedPath, httpPostedFile.FileName);

                    if (!System.IO.Directory.Exists(savedPath))
                    {
                        Directory.CreateDirectory(savedPath);
                    }
                    httpPostedFile.SaveAs(completePath);
                    inputData.BannerIcon = "/Images/" + httpPostedFile.FileName;
                }

                //Form
                Form objForm = new Form
                {
                    SiteId = objSite.SiteId,
                    BannerIcon = imagepath,
                    BackGroundColor = inputData.BackGroundColor,
                    LoginWindowColor = inputData.LoginWindowColor,
                    IsPasswordRequire = Convert.ToBoolean(inputData.IsPasswordRequire),
                    LoginPageTitle = inputData.LoginPageTitle,
                    RegistrationPageTitle = inputData.RegistrationPageTitle,
                    
                    //HtmlCodeForLogin = dynamicHtmlCode
                };
                db.Form.Add(objForm);
                db.SaveChanges();
                var formId = objForm.FormId;

                //Alter table with generating dynamic html code.
                //string dynamicHtmlCode = null;
                if (fieldLabel.Length > 1)
                {
                    int i;
                    for (i = 0; i < dataType.Length; i++)
                    {
                        var datatype = dataType[i];
                        //var controltype = controlType[i];
                        var fieldlabel = fieldLabel[i];
                        string sqlString = "alter table [Users] add" + " " + fieldlabel + " " + datatype + " " + "NULL";
                        db.Database.ExecuteSqlCommand(sqlString);
                        //StringBuilder sb = new StringBuilder(string.Empty);

                        //FormControl objFormControl = new FormControl();
                        //objFormControl.ControlType = controltype;
                        //objFormControl.LabelName = fieldlabel;
                        //objFormControl.FormId = objForm.FormId;
                        //db.FormControl.Add(objFormControl);
                        //db.SaveChanges();
                        ////div start
                        //sb.Append("<div>");
                        //sb.Append("<input type=" + '"' + controltype + '"' + " " + "id=" + '"' + fieldlabel + '"' + " " + "name=" + '"' + fieldlabel + '"' + " " + "placeholder=" + '"' + "Enter" + " " + fieldlabel + '"' + "/>");
                        ////div end
                        //sb.Append("</div>");

                        //dynamicHtmlCode += sb.ToString();
                    }
                }
                //objForm.HtmlCodeForLogin = dynamicHtmlCode;
                //db.Entry(objForm).State = System.Data.Entity.EntityState.Modified;
                //db.SaveChanges();
                return RedirectToAction("Index", "Admin");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Show existing site details.
        /// </summary>
        /// <returns></returns>
        /// Get:Site details.
        public ActionResult SiteDetails()
        {
            var lstSite = (from item in db.Site.ToList()
                           select new SiteViewModel()
                           {
                               CmpName = item.Company.CompanyName,
                               OrgName = item.Company.Organisation.OrganisationName,
                               SiteName = item.SiteName,
                               SiteId = item.SiteId
                           }).ToList();
            return View(lstSite);
        }

        /// <summary>
        /// Populate Site details or form details of existing site Or create new org/comp/field/.
        /// </summary>
        /// <param name="SiteId"></param>
        /// <returns></returns>
        public ActionResult ConfigureSite(int SiteId)
        {
            try
            {
                ViewBag.companies = from item in db.Company.ToList()
                                    select new SelectListItem()
                                    {
                                        Text = item.CompanyName,
                                        Value = item.CompanyId.ToString(),
                                    };
                List<string> columnsList = db.Database.SqlQuery<string>("select column_name from information_schema.columns where table_name = 'users'").ToList();
                FormViewModel objViewModel = new FormViewModel();

                Form objForm = db.Form.FirstOrDefault(m => m.SiteId == SiteId);
                objForm.SiteId = SiteId;
                objViewModel.FormId = objForm.FormId;
                objViewModel.SiteName = db.Site.FirstOrDefault(m => m.SiteId == SiteId).SiteName;
                objViewModel.BannerIcon = objForm.BannerIcon;
                objViewModel.BackGroundColor = objForm.BackGroundColor;
                objViewModel.LoginWindowColor = objForm.LoginWindowColor;
                objViewModel.IsPasswordRequire = objForm.IsPasswordRequire;
                objViewModel.LoginPageTitle = objForm.LoginPageTitle;
                objViewModel.RegistrationPageTitle = objForm.RegistrationPageTitle;
                objViewModel.fieldlabel = columnsList;
                if (db.Site.Any(m => m.SiteId == SiteId))
                {
                    objViewModel.CompanyDdl = (int)db.Site.FirstOrDefault(m => m.SiteId == SiteId).CompanyId;
                }
                objViewModel.FormControls = db.FormControl.Where(m => m.FormId == objForm.FormId).ToList();
                return View(objViewModel);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult UploadFile(FormCollection fc)
        {
            if (Request.Files.Count > 0)
            {
                try
                {
                    var file = Request.Files[fc["BannerIcon"]];
                    byte[] fileBytes = new byte[file.ContentLength];
                    file.InputStream.Read(fileBytes, 0, file.ContentLength);
                    return Json("File Uploaded Successfully!");
                }
                catch (Exception ex)
                {
                    return Json("Error occurred. Error details: " + ex.Message);
                }
            }
            else
            {
                return Json("No files selected.");
            }
        }

        /// <summary>
        /// On Update site Detail Submit
        /// </summary>
        /// <param name="inputData"></param>
        /// <param name="fc"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateSiteAndLoginRegisterConf(FormViewModel inputData, FormCollection fc)
        {
            if (inputData.CompanyName == null)
            {
                string imagepath = null;
                if (Request.Files["BannerIcon"].ContentLength > 0)
                {
                    var httpPostedFile = Request.Files["BannerIcon"];
                    string savedPath = HostingEnvironment.MapPath("/Images/" + inputData.SiteId);
                    imagepath = "/Images/" + inputData.SiteId + "/" + httpPostedFile.FileName;
                    string completePath = Path.Combine(savedPath, httpPostedFile.FileName);

                    if (!System.IO.Directory.Exists(savedPath))
                    {
                        Directory.CreateDirectory(savedPath);
                    }
                    httpPostedFile.SaveAs(completePath);
                    inputData.BannerIcon = "/Images/" + httpPostedFile.FileName;
                }
                //form
                Form objForm = new Form
                {
                    FormId = inputData.FormId,
                    SiteId = inputData.SiteId,
                    BannerIcon = imagepath,
                    IsPasswordRequire=Convert.ToBoolean(inputData.IsPasswordRequire),
                    BackGroundColor = inputData.BackGroundColor,
                    LoginWindowColor = inputData.LoginWindowColor,
                    LoginPageTitle = inputData.LoginPageTitle,
                    RegistrationPageTitle = inputData.RegistrationPageTitle
                };
                db.Entry(objForm).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Index", "Admin");
        }

        [HttpPost]
        public JsonResult SaveFormControls(FormControlViewModel model)
        {
            try
            {
                Form objForm = db.Form.FirstOrDefault(m => m.FormId == model.FormId);
                StringBuilder sb = new StringBuilder(String.Empty);
                if (model.controlType == "dropdown")
                {
                    sb.Append("<div>");
                    sb.Append("<select name=" + '"' + model.fieldlabel + '"' + ">");
                    foreach (var item in model.arrayValue)
                    {
                        sb.Append("<option value=" + '"' + item + '"' + ">" + item + "</option>");
                    }
                    sb.Append("</select>");
                    sb.Append("</div>");
                }
                else if (model.controlType == "checkbox")
                {
                    sb.Append("<div>");
                    foreach (var item in model.arrayValue)
                    {
                        sb.Append("<input type=" + '"' + model.controlType + '"' + " " + "id=" + '"' + model.fieldlabel + '"' + " " + "name=" + '"' + model.fieldlabel + '"' + " " + "value=" + '"' + item + '"' + ">" + item);
                    }
                    sb.Append("</div>");
                }
                else if (model.controlType == "radio")
                {
                    sb.Append("<div>");
                    foreach (var item in model.arrayValue)
                    {
                        sb.Append("<input type=" + '"' + model.controlType + '"' + " " + "id=" + '"' + model.fieldlabel + '"' + " " + "name=" + '"' + model.fieldlabel + '"' + " " + "value=" + '"' + item + '"' + ">" + item);
                    }
                    sb.Append("</div>");
                }
                else
                {
                    //div start
                    //sb.Append("<div>");
                    //sb.Append("<input type=" + '"' + model.controlType + '"' + " " + "id=" + '"' + model.fieldlabel + '"' + " " + "name=" + '"' + model.fieldlabel + '"' + " " + "placeholder=" + '"' + "Enter" + " " + model.fieldlabel + '"' + "/>");
                    //sb.Append("</div>");
                    //div end
                    //div start
                    sb.Append("<div class='form-group'>");
                    sb.Append("<label class='control-label col-sm-2'>"+ model.fieldlabel + "</label>");
                    sb.Append("<div class='col-sm-10'>");
                    sb.Append("<input type=" + '"' + model.controlType + '"' +'"'+""+ "class='form-control'"+" "+ "placeholder = " + '"' + "Enter" + " " + model.fieldlabel + '"' + " /> ");
                    sb.Append("</div>");
                                    
                                
                    sb.Append("</div>");
                    //div end
                }

                FormControl objFormControl = new FormControl();
                objFormControl.ControlType = model.controlType;
                objFormControl.LabelName = model.fieldlabel;
                objFormControl.FormId = model.FormId;
                objFormControl.HtmlString = sb.ToString();
                db.FormControl.Add(objFormControl);

                //db.Entry(objForm).State = System.Data.Entity.EntityState.Modified;
                //objForm.HtmlCodeForLogin = sb.ToString();
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return Json("Failure");
            }
            return Json("Success");
        }

        [HttpGet]
        public ActionResult DeleteFormControl(int Id)
        {
            FormControl objFormControl = null;
            Form objForm = null;
            try
            {
                objFormControl = db.FormControl.FirstOrDefault(m => m.FormControlId == Id);
                objForm = db.FormControl.FirstOrDefault(m => m.FormControlId == Id).Forms;
                db.FormControl.Remove(objFormControl);
                db.SaveChanges();
            }
            catch (Exception ex)
            {

            }
            return RedirectToAction("ConfigureSite", new { SiteId = objForm.SiteId });
        }

        //public ActionResult FormLogin()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public ActionResult OnFormLoginSubmit(FormCollection form)
        //{
        //    Site result = db.Site.First();

        //    Form objForm = new Form
        //    {
        //        FormName = form["formName"],
        //        SiteId = result.SiteId
        //    };
        //    db.Form.Add(objForm);
        //    var res = db.SaveChanges();
        //    var id = objForm.FormId;

        //    FormControl objFormControl = new FormControl
        //    {
        //        FormId = id,
        //        ControlType = form["controlType"],
        //        LabelName = form["labelName"],
        //        SiteUrl = form["siteUrl"]
        //    };
        //    db.FormControl.Add(objFormControl);
        //    db.SaveChanges();
        //    return Content("hi");
        //}

        // GET: AdminIndex
        public ActionResult Index()
        {
            //GetFormDataController objGetFormDataController = new GetFormDataController();
            //var result = objGetFormDataController.GetSiteDetailsTest();
            //string orgCompList=result.Replace(@"\", "");
            //ViewData["OrgCompList"] = result;
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult UserDetails()
        {
            var userList = (from item in db.Users.ToList()
                            select new UserViewModel()
                            {
                                UserId = item.UserId,
                                UserName = item.UserName,
                                FirstName = item.FirstName,
                                LastName = item.LastName,
                                CreationDate = item.CreationDate
                            }).ToList();
            return View(userList);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
       
        public ActionResult UserWithProfile(int UserId)
        {
            var userDetail = db.Users.FirstOrDefault(m => m.UserId == UserId);
            UserViewModel objUserViewModel = new UserViewModel();
            //objUserViewModel.MobileNumber = userDetail.MobileNumber;
            objUserViewModel.Gender = userDetail.Gender;
            objUserViewModel.AgeRange = userDetail.Age;
            return View(objUserViewModel);
        }

        public ActionResult UpdatePassword(int UserId)
        {
            return View();
        }

        public ActionResult MacAddress(int UserId)
        {
            return View();
        }

        //#region
        //private void InsertIntoOrganisation(FormViewModel inputData)
        //{
        //    try
        //    {
        //        Organisation objOrganisation = new Organisation
        //        {
        //            OrganisationName = inputData.OrganisationName
        //        };
        //        db.Organisation.Add(objOrganisation);
        //        db.SaveChanges();
        //        var orgId = objOrganisation.OrganisationId;
        //    }
        //    catch(Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        //private void InsertIntoCompany(FormViewModel inputData)
        //{
        //    try
        //    {
        //        Company objCompany = new Company
        //        {
        //            CompanyName = inputData.CompanyName,
        //            //OrganisationId = orgId,
        //        };
        //        db.Company.Add(objCompany);
        //        db.SaveChanges();
        //        var compId = objCompany.CompanyId;
        //    }
        //    catch(Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        //private void InsertIntoSite(FormViewModel inputData)
        //{
        //    try
        //    {
        //        Site objSite = new Site
        //        {
        //            SiteName = inputData.SiteName,
        //            //CompanyId = compId
        //        };
        //        db.Site.Add(objSite);
        //        db.SaveChanges();
        //        var siteId = objSite.SiteId;
        //    }
        //    catch(Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        //private JsonResult InsertIntoForm(FormViewModel inputData)
        //{
        //    try
        //    {
        //        Form objForm = new Form
        //        {
        //            SiteId = siteId,
        //            BannerIcon = imagepath,
        //            BackGroundColor = inputData.BackGroundColor,
        //            LoginWindowColor = inputData.LoginWindowColor,
        //            IsPasswordRequire = Convert.ToBoolean(inputData.IsPasswordRequire),
        //            LoginPageTitle = inputData.LoginPageTitle,
        //            RegistrationPageTitle = inputData.RegistrationPageTitle,
        //            //HtmlCodeForLogin = dynamicHtmlCode
        //        };
        //        db.Form.Add(objForm);
        //        db.SaveChanges();
        //        formId = objForm.FormId;
        //    }
        //    catch(Exception ex)
        //    {
        //        throw ex;
        //    }
        //    return Json(formId);
        //}

        //private void UpadteForm(FormViewModel inputData)
        //{
        //    try
        //    {
        //        Form objForm = new Form
        //        {
        //            FormId = inputData.FormId,
        //            SiteId = inputData.SiteId,
        //            LoginPageTitle = inputData.LoginPageTitle,
        //            RegistrationPageTitle = inputData.RegistrationPageTitle
        //        };
        //        db.Entry(objForm).State = System.Data.Entity.EntityState.Modified;
        //        db.SaveChanges();
        //    }
        //    catch(Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        //#endregion
    }
}