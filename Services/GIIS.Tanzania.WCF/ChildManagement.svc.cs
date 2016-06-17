﻿//*******************************************************************************
//Copyright 2015 TIIS  Webservices - Tanzania Immunization Information System
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//******************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using GIIS.DataLayer;
using System.Globalization;
using System.Net;
using System.IO;
using System.ServiceModel.Web;

namespace GIIS.Tanzania.WCF
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "ChildManagement" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select ChildManagement.svc or ChildManagement.svc.cs at the Solution Explorer and start debugging.
    public class ChildManagement : IChildManagement
    {

        public string sampleGCMTest(string message , string regId)
        {

            string stringregIds = null;
            List<string> regIDs = new List<string>();
            
            //string[] ids = regId.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string[] ids = regId.Split(',');
            for (int i = 0; i < ids.Length; i++) {
                string value = ids[i].Trim();
                regIDs.Add(value);
            }
            
            //Here I add the registrationID that I used in Method #1 to regIDs
            regIDs.Add(regId);

            //Then I use 
            stringregIds = string.Join("\",\"", regIDs);
            //To Join the values (if ever there are more than 1) with quotes and commas for the Json format below

            try
            {
                string GoogleAppID = "AIzaSyBgsthTTTiunMtHV5XT1Im6bl17i5rGR94";
                var SENDER_ID = "967487253557";
                var value = message;
                WebRequest tRequest;
                tRequest = WebRequest.Create("https://android.googleapis.com/gcm/send");
                tRequest.Method = "post";
                tRequest.ContentType = "application/json";
                tRequest.Headers.Add(string.Format("Authorization: key={0}", GoogleAppID));

                tRequest.Headers.Add(string.Format("Sender: id={0}", SENDER_ID));

                string postData = "{\"collapse_key\":\"score_update\",\"time_to_live\":108,\"delay_while_idle\":true,\"data\": { \"message\" : " + "\"" + value + "\",\"time\": " + "\"" + System.DateTime.Now.ToString() + "\"},\"registration_ids\":[\"" + stringregIds + "\"]}";

                Byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                tRequest.ContentLength = byteArray.Length;

                Stream dataStream = tRequest.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                WebResponse tResponse = tRequest.GetResponse();

                dataStream = tResponse.GetResponseStream();

                StreamReader tReader = new StreamReader(dataStream);

                String sResponseFromServer = tReader.ReadToEnd();

                HttpWebResponse httpResponse = (HttpWebResponse)tResponse;
                string statusCode = httpResponse.StatusCode.ToString();

                tReader.Close();
                dataStream.Close();
                tResponse.Close();

                return sResponseFromServer;
            }
            catch
            {
                //throw new WebFaultException<string>("Error", HttpStatusCode.ServiceUnavailable);
                return "error sending data to GCM server. Service unavailable";
            }

            
        }

        public IntReturnValue RegisterChildWithoutAppointments(string barcodeId, string firstname1, string lastname1, DateTime birthdate, bool gender,
            int healthFacilityId, int birthplaceId, int domicileId, string address, string phone, string motherFirstname,
            string motherLastname, string notes, int userId, DateTime modifiedOn)
        {
            Child o = new Child();

            o.Firstname1 = firstname1;
            o.Lastname1 = lastname1;

            o.Birthdate = birthdate;
            o.Gender = gender;

            o.HealthcenterId = healthFacilityId;
            o.BirthplaceId = birthplaceId;
            o.DomicileId = domicileId;

            o.Address = address;
            o.Phone = phone;
            o.MotherFirstname = motherFirstname;
            o.MotherLastname = motherLastname;
            o.Notes = notes;
            o.ModifiedOn = modifiedOn;
            o.ModifiedBy = userId;

            o.SystemId = DateTime.Now.ToString("yyMMddhhmmss");
            o.BarcodeId = barcodeId;

            o.StatusId = 1;

            int childInserted = Child.Insert(o);

            IntReturnValue irv = new IntReturnValue();
            irv.id = childInserted;
            return irv;

        }

        public IntReturnValue RegisterChildWithAppoitments(string barcodeId, string firstname1, string firstname2, string lastname1, DateTime birthdate, bool gender,
            int healthFacilityId, int birthplaceId, int domicileId, string address, string phone, string motherFirstname,
            string motherLastname, string notes, int userId, DateTime modifiedOn)
        {
            Child o = new Child();

            o.Firstname1 = firstname1;
            o.Lastname1 = lastname1;
            o.Firstname2 = firstname2;
            o.Birthdate = birthdate;
            o.Gender = gender;

            //if (childExists(o.Lastname1, o.Gender, o.Birthdate))
            //    return -1;

            o.HealthcenterId = healthFacilityId;
            o.BirthplaceId = birthplaceId;
            o.DomicileId = domicileId;

            o.Address = address;
            o.Phone = phone;
            o.MotherFirstname = motherFirstname;
            o.MotherLastname = motherLastname;
            o.Notes = notes;
            o.ModifiedOn = modifiedOn;
            o.ModifiedBy = userId;

            o.SystemId = DateTime.Now.ToString("yyMMddhhmmss");
            o.BarcodeId = barcodeId;
            o.IsActive = true;
            o.StatusId = 1;

            int childInserted = Child.Insert(o);

            if (childInserted > 0)
            {
                //add appointments
                VaccinationAppointment.InsertVaccinationsForChild(childInserted, userId);
            }

            IntReturnValue irv = new IntReturnValue();
            irv.id = childInserted;
            return irv;
        }

        public IntReturnValue UpdateChild(string barcode, string firstname1, string firstname2, string lastname1, DateTime birthdate, bool gender,
             int healthFacilityId, int birthplaceId, int domicileId, int statusId, string address, string phone, string motherFirstname,
             string motherLastname, string notes, int userId, int childId, DateTime modifiedOn)
        {
           Child o = null;
            int n;
            int healthcenter = 0;
            int datediff = Int32.MaxValue;
           bool isNumeric = int.TryParse(childId.ToString(), out n);
            if (isNumeric && childId != 0)
                o = Child.GetChildById(childId);
            else if (!string.IsNullOrEmpty(barcode))
                 o = Child.GetChildByBarcode(barcode);
         
           if (o != null)
           {
               o.Firstname1 = firstname1;
               o.Lastname1 = lastname1;
               o.Firstname2 = firstname2;
               
               if (o.Birthdate != birthdate)
                   datediff = birthdate.Subtract(o.Birthdate).Days;

               o.Birthdate = birthdate;
               o.Gender = gender;
               
               if (o.HealthcenterId != healthFacilityId)
               {
                   healthcenter = healthFacilityId;
               }
               o.HealthcenterId = healthFacilityId;
               o.BirthplaceId = birthplaceId;
               o.DomicileId = domicileId;
               o.CommunityId = null;
               o.StatusId = statusId;
               o.Address = address;
               o.Phone = phone;
               o.MotherFirstname = motherFirstname;
               o.MotherLastname = motherLastname;
               o.Notes = notes;
               o.ModifiedOn = modifiedOn; // DateTime.Now;
               o.ModifiedBy = userId;
               o.BarcodeId = barcode;
           }
            int childUpdated = Child.Update(o);

            if (childUpdated > 0)
            {
                bool appstatus = true;
                if (o.StatusId != 1)
                    appstatus = false;

                List<VaccinationAppointment> applist = VaccinationAppointment.GetVaccinationAppointmentsByChildNotModified(childId);
                List<VaccinationAppointment> applistall = VaccinationAppointment.GetVaccinationAppointmentsByChild(childId);
                if (!appstatus)
                {
                    foreach (VaccinationAppointment app in applist)
                        VaccinationAppointment.Update(appstatus, app.Id);
                }

                if (healthcenter != 0)
                {
                    foreach (VaccinationAppointment app in applist)
                    {
                        VaccinationAppointment.Update(o.HealthcenterId, app.Id);
                        GIIS.DataLayer.VaccinationEvent.Update(app.Id, o.HealthcenterId);
                    }
                }
                if (datediff != Int32.MaxValue)
                {
                    bool done = false;
                    foreach (VaccinationAppointment app in applistall)
                    {
                        GIIS.DataLayer.VaccinationEvent ve = GIIS.DataLayer.VaccinationEvent.GetVaccinationEventByAppointmentId(app.Id)[0];
                        if (ve.VaccinationStatus || ve.NonvaccinationReasonId != 0)
                        {
                            done = true;
                            break;
                        }
                    }

                    foreach (VaccinationAppointment app in applist)
                    {
                        if (done)
                            break;
                        VaccinationAppointment.Update(app.ScheduledDate.AddDays(datediff), app.Id);
                        GIIS.DataLayer.VaccinationEvent.Update(app.Id, app.ScheduledDate.AddDays(datediff));
                    }

                }
            }

            IntReturnValue irv = new IntReturnValue();
            irv.id = childUpdated;
            return irv;

        }

        public IntReturnValue RemoveChild(int id)
        {
            int removed = Child.Remove(id);
            IntReturnValue irv = new IntReturnValue();
            irv.id = removed;
            return irv;
        }

        public IntReturnValue DeleteChild(int id)
        {
            int deleted = Child.Delete(id);
            IntReturnValue irv = new IntReturnValue();
            irv.id = deleted;
            return irv;
        }

        public List<Child> FindDublication(bool birthdateFlag, bool firstnameFlag, bool genderFlag, int healthFacilityId)
        {
            List<Child> dublications = Child.GetDuplications(birthdateFlag, firstnameFlag, genderFlag, healthFacilityId);

            return dublications;
        }

        public List<Child> Search(int statusId, DateTime birthdateFrom, DateTime birthdateTo, string firstname1, string lastname1, string otherId, int healthFacilityId,
            int birthplaceId, int communityId, int domicileId, string address, string phone, string mobile)
        {
            int max = Int32.MaxValue;
            int start = 0;

            string idFields = null;
            string motherFirstname = null;
            string motherLastname = null;
            string systemId = null;
            string barcodeId = null;
            string tempId = null;

            List<Child> childList = Child.GetPagedChildList(statusId, birthdateFrom, birthdateTo, firstname1, lastname1, idFields,
                healthFacilityId.ToString(), birthplaceId, communityId, domicileId, motherFirstname, motherLastname, systemId, barcodeId, tempId,
                ref max, ref start);

            return childList;
        }

        public List<ChildResults> Search(string where)
        {
            int statusId = 1;
            DateTime birthdateFrom = new DateTime();
            DateTime birthdateTo = new DateTime();

            string firstname1 = null;
            string lastname1 = null;

            int birthplaceId = 0;
            int communityId = 0;
            int domicileId = 0;

            string idFields = null;
            string motherFirstname = null;
            string motherLastname = null;
            string systemId = null;
            string barcodeId = null;
            string tempId = null;

            string healthFacilityId = null;

            string[] s = where.Split('!');
            foreach (string s1 in s)
            {
                if (s1.ToLower().Contains("firstname1"))
                    firstname1 = s1.Substring(s1.IndexOf('=') + 1).ToUpper();

                if (s1.ToLower().Contains("lastname1"))
                    lastname1 = s1.Substring(s1.IndexOf('=') + 1).ToUpper();

                if (s1.ToLower().Contains("motherfirstname"))
                    motherFirstname = s1.Substring(s1.IndexOf('=') + 1).ToUpper();

                if (s1.ToLower().Contains("motherlastname"))
                    motherLastname = s1.Substring(s1.IndexOf('=') + 1).ToUpper();

                //if (s1.ToLower().Contains("birthdate"))
                //{
                //    string birthdate = s1.Substring(s1.IndexOf('=') + 1).ToUpper();
                //    _where += string.Format(@" AND ""BIRTHDATE"" = '{0}'", birthdate);
                //}

                if (s1.ToLower().Contains("healthfacilityid"))
                    healthFacilityId = s1.Substring(s1.IndexOf('=') + 1);

                if (s1.ToLower().Contains("birthplaceid"))
                    Int32.TryParse(s1.Substring(s1.IndexOf('=') + 1), out birthplaceId);


                if (s1.ToLower().Contains("domicileid"))
                    Int32.TryParse(s1.Substring(s1.IndexOf('=') + 1), out domicileId);

                if (s1.ToLower().Contains("statusid"))
                    Int32.TryParse(s1.Substring(s1.IndexOf('=') + 1), out statusId);

                if (s1.ToLower().Contains("birthdatefrom"))
                {
                    string bd = s1.Substring(s1.IndexOf('=') + 1).ToUpper();
                    birthdateFrom = DateTime.ParseExact(bd, "yyyy-MM-dd", CultureInfo.CurrentCulture);
                }

                if (s1.ToLower().Contains("birthdateto"))
                {
                    string bd = s1.Substring(s1.IndexOf('=') + 1).ToUpper();
                    birthdateTo = DateTime.ParseExact(bd, "yyyy-MM-dd", CultureInfo.CurrentCulture);
                }
                if (s1.ToLower().Contains("barcode"))
                    barcodeId = s1.Substring(s1.IndexOf('=') + 1).ToUpper();
                if (s1.ToLower().Contains("tempid"))
                    tempId = s1.Substring(s1.IndexOf('=') + 1).ToUpper();

            }

            int max = Int32.MaxValue;
            int start = 0;

            List<Child> childList = Child.GetPagedChildList(statusId, birthdateFrom, birthdateTo, firstname1, lastname1, idFields,
                healthFacilityId, birthplaceId, communityId, domicileId, motherFirstname, motherLastname, systemId, barcodeId, tempId,
                ref max, ref start);
            List<ChildResults> chlist = new List<ChildResults>();
            foreach (Child ch in childList)
            {
                ChildResults chr = new ChildResults();
                chr.Id = ch.Id;
                chr.Firstname1 = ch.Firstname1;
                chr.Lastname1 = ch.Lastname1;
                chr.BarcodeId = ch.BarcodeId;
                chr.MotherFirstname = ch.MotherFirstname;
                chr.MotherLastname = ch.MotherLastname;
                chr.Gender = ch.Gender;
                chr.Birthdate = ch.Birthdate;
                chr.HealthcenterId = ch.Healthcenter.Name;
                if (ch.Domicile != null)
                    chr.DomicileId = ch.Domicile.Name;
                chr.Firstname2 = ch.Firstname2;
                chlist.Add(chr);
            }
            return chlist;
        }

        public List<Child> GetChildByHealthFacilityId(int healthFacilityId)
        {
            List<Child> childList = Child.GetChildByHealthFacilityId(healthFacilityId);
            return childList;
        }

        public List<ChildEntity> GetChildrenByHealthFacility(int healthFacilityId)
        {
            List<Child> childList = Child.GetChildByHealthFacilityId(healthFacilityId);


            List<ChildEntity> ceList = new List<ChildEntity>();

            foreach (Child child in childList)
            {
                List<GIIS.DataLayer.VaccinationEvent> veList = GIIS.DataLayer.VaccinationEvent.GetChildVaccinationEvent(child.Id);
                List<GIIS.DataLayer.VaccinationAppointment> vaList = GIIS.DataLayer.VaccinationAppointment.GetVaccinationAppointmentsByChild(child.Id);

                ChildEntity ce = new ChildEntity();
                ce.childEntity = child;
                ce.vaList = vaList; // GetVaccinationAppointment(vaList);
                ce.veList = veList; // GetVaccinationEvent(veList);
                ceList.Add(ce);
            }

            return ceList;
        }

        public List<Child> GetOnlyChildrenByHealthFacility(int healthFacilityId)
        {
            List<Child> childList = Child.GetChildByHealthFacilityId(healthFacilityId);


            return childList;
        }

        public ChildListEntity GetOnlyChildrenDataByHealthFacility(int healthFacilityId)
        {
            if (healthFacilityId > 0)
            {
                ChildListEntity cle = new ChildListEntity();

                List<Child> chList = Child.GetChildByHealthFacilityId(healthFacilityId);
                List<GIIS.DataLayer.VaccinationAppointment> valist = GIIS.DataLayer.VaccinationAppointment.GetVaccinationAppointmentsByHealthFacility (healthFacilityId);
                List<GIIS.DataLayer.VaccinationEvent> velist = GIIS.DataLayer.VaccinationEvent.GetVaccinationEventByHealthFacility(healthFacilityId);
               
                cle.childList = chList;
                cle.vaList = valist;
                cle.veList = velist;

                return cle;
            }
            else
                return null;
        }

        public ChildListEntity GetChildrenByHealthFacilitySinceLastLogin(int idUser)
        {
            if (idUser > 0)
            {
                User user = User.GetUserById(idUser);
                ChildListEntity cle = new ChildListEntity();

                List<Child> childList = Child.GetChildByHealthFacilityIdSinceLastLogin(idUser);
                cle.childList = childList;

                List<GIIS.DataLayer.VaccinationAppointment> vaList = GIIS.DataLayer.VaccinationAppointment.GetVaccinationAppointmentsByChild(user.Lastlogin, user.Id);
                cle.vaList = vaList;

                List<GIIS.DataLayer.VaccinationEvent> veList = GIIS.DataLayer.VaccinationEvent.GetChildVaccinationEvent(user.Lastlogin, user.Id);
                cle.veList = veList;

                //List<ChildEntity> ceList = new List<ChildEntity>();

                //foreach (Child child in childList)
                //{

                //    ChildEntity ce = new ChildEntity();
                //    ce.childEntity = child;
                //    ce.vaList = vaList; // GetVaccinationAppointment(vaList);
                //    ce.veList = veList; // GetVaccinationEvent(veList);
                //    ceList.Add(ce);
                //}

                //return ceList;
                 return cle;
            }
            else
                return null;
    }
        
        //public List<ChildEntity> GetChildrenByHealthFacilityBeforeLastLogin(int idUser)
        //{
        //    User user = User.GetUserById(idUser);

        //    List<Child> childList = Child.GetChildByHealthFacilityIdBeforeLastLogin(idUser);

        //    List<ChildEntity> ceList = new List<ChildEntity>();

        //    foreach (Child child in childList)
        //    {
        //        List<GIIS.DataLayer.VaccinationEvent> veList = GIIS.DataLayer.VaccinationEvent.GetChildVaccinationEventBefore(child.Id, user.Lastlogin, user.PrevLogin, user.Id);
        //        List<GIIS.DataLayer.VaccinationAppointment> vaList = GIIS.DataLayer.VaccinationAppointment.GetVaccinationAppointmentsByChildBefore(child.Id, user.Lastlogin, user.Id);

        //        ChildEntity ce = new ChildEntity();
        //        ce.childEntity = child;
        //        ce.vaList = vaList; // GetVaccinationAppointment(vaList);
        //        ce.veList = veList; // GetVaccinationEvent(veList);
        //        ceList.Add(ce);
        //    }

        //    return ceList;
        //}

        public ChildListEntity GetChildrenByHealthFacilityBeforeLastLogin(int idUser)
        {
            if (idUser > 0)
            {
                User user = User.GetUserById(idUser);
                ChildListEntity cle = new ChildListEntity();

                List<Child> childList = Child.GetChildByHealthFacilityIdBeforeLastLogin(idUser);
                cle.childList = childList;

                List<GIIS.DataLayer.VaccinationAppointment> vaList = GIIS.DataLayer.VaccinationAppointment.GetVaccinationAppointmentsByChildBefore(user.Lastlogin, user.PrevLogin, user.Id);
                cle.vaList = vaList;

                List<GIIS.DataLayer.VaccinationEvent> veList = GIIS.DataLayer.VaccinationEvent.GetChildVaccinationEventBefore(user.Lastlogin, user.PrevLogin , user.Id);
                cle.veList = veList;
                return cle;
            }
            else
                return null;
           
        }

        public ChildListEntity GetChildrenByHealthFacilityDayFirstLogin(int idUser)
        {
            if (idUser > 0)
            {
                User user = User.GetUserById(idUser);
                ChildListEntity cle = new ChildListEntity();

                List<Child> childList = Child.GetChildByHealthFacilityIdDayFirstLogin(idUser);
                cle.childList = childList;

                List<GIIS.DataLayer.VaccinationAppointment> vaList = GIIS.DataLayer.VaccinationAppointment.GetVaccinationAppointmentsByChildDayFirstLogin(user.Lastlogin.AddDays(-1), user.Id);
                cle.vaList = vaList;

                List<GIIS.DataLayer.VaccinationEvent> veList = GIIS.DataLayer.VaccinationEvent.GetChildVaccinationEventDayFirstLogin(user.Lastlogin.AddDays(-1), user.Id);
                cle.veList = veList;
                return cle;
            }
            else
                return null;
        }

        //public List<ChildEntity> GetChildrenByHealthFacilityDayFirstLogin(int idUser)
        //{
        //    User user = User.GetUserById(idUser);

        //    List<Child> childList = Child.GetChildByHealthFacilityIdDayFirstLogin(idUser);

        //    List<ChildEntity> ceList = new List<ChildEntity>();

        //    foreach (Child child in childList)
        //    {
        //        List<GIIS.DataLayer.VaccinationEvent> veList = GIIS.DataLayer.VaccinationEvent.GetChildVaccinationEventDayFirstLogin(child.Id, user.Lastlogin.AddDays(-1), user.Id);
        //        List<GIIS.DataLayer.VaccinationAppointment> vaList = GIIS.DataLayer.VaccinationAppointment.GetVaccinationAppointmentsByChildDayFirstLogin(child.Id, user.Lastlogin.AddDays(-1));

        //        ChildEntity ce = new ChildEntity();
        //        ce.childEntity = child;
        //        ce.vaList = vaList; // GetVaccinationAppointment(vaList);
        //        ce.veList = veList; // GetVaccinationEvent(veList);
        //        ceList.Add(ce);
        //    }

        //    return ceList;
        //}
        #region Helper
        protected bool childExists(string lastname, bool gender, DateTime birthdate)
        {
            return (Child.GetPersonByLastnameBirthdateGender(lastname, birthdate, gender) != null);
        }
        protected bool IdentificationNo1Exists(string id)
        {
            return (Child.GetPersonIdentification1(id) != null);
        }
        protected bool IdentificationNo2Exists(string id)
        {
            return (Child.GetPersonIdentification2(id) != null);
        }
        protected bool IdentificationNo3Exists(string id)
        {
            return (Child.GetPersonIdentification3(id) != null);
        }

        private List<GIIS.DataLayer.VaccinationEvent> GetVaccinationEvent(List<GIIS.DataLayer.VaccinationEvent> veList)
        {
            List<GIIS.DataLayer.VaccinationEvent> tmp = new List<GIIS.DataLayer.VaccinationEvent>();

            return tmp;
        }
        private List<GIIS.DataLayer.VaccinationAppointment> GetVaccinationAppointment(List<GIIS.DataLayer.VaccinationAppointment> vaList)
        {
            List<GIIS.DataLayer.VaccinationAppointment> tmp = new List<GIIS.DataLayer.VaccinationAppointment>();

            return tmp;
        }

        private List<ChildEntity> GetChildrenWithAppointmentAndEvents(List<Child> childList)
        {
            List<ChildEntity> ceList = new List<ChildEntity>();

            foreach (Child child in childList)
            {
                List<GIIS.DataLayer.VaccinationEvent> veList = new List<DataLayer.VaccinationEvent>(); // GIIS.DataLayer.VaccinationEvent.GetChildVaccinationEvent(child.Id, child.ModifiedOn);
                List<GIIS.DataLayer.VaccinationAppointment> vaList = new List<VaccinationAppointment>(); // GIIS.DataLayer.VaccinationAppointment.GetVaccinationAppointmentsByChild(child.Id, child.ModifiedOn);

                ChildEntity ce = new ChildEntity();
                ce.childEntity = child;
                ce.vaList = GetVaccinationAppointment(vaList);
                ce.veList = GetVaccinationEvent(veList);
                ceList.Add(ce);
            }

            return ceList;
        }
        #endregion

        public List<ChildEntity> SearchByBarcode(string barcodeId)
        {
            GIIS.DataLayer.Child child = GIIS.DataLayer.Child.GetChildByBarcode(barcodeId);

            List<ChildEntity> ceList = new List<ChildEntity>();

            if (child != null)
            {
                List<GIIS.DataLayer.VaccinationEvent> veList = GIIS.DataLayer.VaccinationEvent.GetChildVaccinationEvent(child.Id);
                List<GIIS.DataLayer.VaccinationAppointment> vaList = GIIS.DataLayer.VaccinationAppointment.GetVaccinationAppointmentsByChild(child.Id);

                ChildEntity ce = new ChildEntity();
                ce.childEntity = child;
                ce.vaList = vaList; // GetVaccinationAppointment(vaList);
                ce.veList = veList; // GetVaccinationEvent(veList);
                ceList.Add(ce);
            }

            return ceList;
        }

        public List<ChildEntity> SearchByTempId(string tempId)
        {
            Child child = Child.GetChildByTempId(tempId);

            List<ChildEntity> ceList = new List<ChildEntity>();

            if (child != null)
            {
                List<GIIS.DataLayer.VaccinationEvent> veList = GIIS.DataLayer.VaccinationEvent.GetChildVaccinationEvent(child.Id);
                List<GIIS.DataLayer.VaccinationAppointment> vaList = GIIS.DataLayer.VaccinationAppointment.GetVaccinationAppointmentsByChild(child.Id);

                ChildEntity ce = new ChildEntity();
                ce.childEntity = child;
                ce.vaList = GetVaccinationAppointment(vaList);
                ce.veList = GetVaccinationEvent(veList);
                ceList.Add(ce);
            }

            return ceList;
        }

        public List<ChildEntity> SearchByNameAndMother(string firstname1, string lastname1, string motherfirstname, string motherlastname)
        {
            int max = Int32.MaxValue;
            int start = 0;

            int statusId = 0;
            DateTime birthdateFrom = new DateTime();
            DateTime birthdateTo = new DateTime();

            int birthplaceId = 0;
            int communityId = 0;
            int domicileId = 0;

            string idFields = null;
            string systemId = null;
            string barcodeId = null;
            string tempId = null;

            int healthFacilityId = 0;

            List<Child> childList = Child.GetPagedChildList(statusId, birthdateFrom, birthdateTo, firstname1, lastname1, idFields,
                healthFacilityId.ToString(), birthplaceId, communityId, domicileId, motherfirstname, motherlastname, systemId, barcodeId, tempId,
                ref max, ref start);

            return GetChildrenWithAppointmentAndEvents(childList);
        }

        public List<ChildEntity> SearchByName(string firstname1, string lastname1)
        {
            int max = Int32.MaxValue;
            int start = 0;

            int statusId = 0;
            DateTime birthdateFrom = new DateTime();
            DateTime birthdateTo = new DateTime();

            int birthplaceId = 0;
            int communityId = 0;
            int domicileId = 0;

            string motherfirstname = null;
            string motherlastname = null;
            string idFields = null;
            string systemId = null;
            string barcodeId = null;
            string tempId = null;

            string healthFacilityId = "";

            List<Child> childList = Child.GetPagedChildList(statusId, birthdateFrom, birthdateTo, firstname1, lastname1, idFields,
                healthFacilityId, birthplaceId, communityId, domicileId, motherfirstname, motherlastname, systemId, barcodeId, tempId,
                ref max, ref start);

            return GetChildrenWithAppointmentAndEvents(childList);
        }

        public List<ChildEntity> SearchByDate(DateTime birthdatefrom, DateTime birthdateto, int birthplaceid, int domicileid)
        {
            int max = Int32.MaxValue;
            int start = 0;

            int statusId = 0;
            int communityId = 0;

            string firstname1 = null;
            string lastname1 = null;
            string motherfirstname = null;
            string motherlastname = null;
            string idFields = null;
            string systemId = null;
            string barcodeId = null;
            string tempId = null;

            int healthFacilityId = 0;

            List<Child> childList = Child.GetPagedChildList(statusId, birthdatefrom, birthdateto, firstname1, lastname1, idFields,
                healthFacilityId.ToString(), birthplaceid, communityId, domicileid, motherfirstname, motherlastname, systemId, barcodeId, tempId,
                ref max, ref start);

            return GetChildrenWithAppointmentAndEvents(childList);
        }

        public bool ChildExists(string where)
        {
            string firstname1 = null;
            string lastname1 = null;
            string motherFirstname = null;
            string motherLastname = null;
            DateTime birthdate = new DateTime();
            bool gender = new bool();

            string[] s = where.Split('!');
            foreach (string s1 in s)
            {
                if (s1.Contains("lastname1"))
                {
                    lastname1 = s1.Substring(s1.IndexOf('=') + 1).ToUpper();
                }

                if (s1.Contains("firstname1"))
                {
                    lastname1 = s1.Substring(s1.IndexOf('=') + 1).ToUpper();
                }

                if (s1.Contains("motherFirstname"))
                {
                    motherFirstname = s1.Substring(s1.IndexOf('=') + 1).ToUpper();
                }

                if (s1.Contains("motherLastname"))
                {
                    motherLastname = s1.Substring(s1.IndexOf('=') + 1).ToUpper();
                }

                if (s1.Contains("birthdate"))
                {
                    string bd = s1.Substring(s1.IndexOf('=') + 1).ToUpper();
                    birthdate = DateTime.ParseExact(bd, "yyyy-MM-dd", CultureInfo.CurrentCulture);
                }

                if (s1.Contains("gender"))
                {
                    string g = s1.Substring(s1.IndexOf('=') + 1).ToUpper();
                    gender = g == "M" ? true : false;
                }
            }



            int count = Child.ChildExists(firstname1, lastname1, motherFirstname, motherLastname, birthdate, gender);

            return (count >= 1);
        }

        public bool ChildExistsByLastnameAndBirthdate(string lastname1, DateTime birthdate)
        {
            string firstname1 = null;
            string motherFirstname = null;
            string motherLastname = null;
            bool gender = new bool();

            int count = Child.ChildExists(firstname1, lastname1, motherFirstname, motherLastname, birthdate, gender);

            return (count >= 1);
        }

        public bool ChildExistsByLastnameAndBirthdateAndGender(string lastname1, string gender, DateTime birthdate)
        {
            string firstname1 = null;
            string motherFirstname = null;
            string motherLastname = null;
            bool _gender = (gender == "M");

            int count = Child.ChildExists(firstname1, lastname1, motherFirstname, motherLastname, birthdate, _gender);

            return (count >= 1);
        }

        public bool ChildExistsByMotherAndBirthdateAndGender(string lastname1, string motherFirstname, string motherLastname, string gender, DateTime birthdate)
        {
            string firstname1 = null;
            bool _gender = (gender == "M");

            int count = Child.ChildExists(firstname1, lastname1, motherFirstname, motherLastname, birthdate, _gender);

            return (count >= 1);
        }

        public IntReturnValue RegisterChildWeight(int childId, DateTime date, double weight, DateTime modifiedOn, int modifiedBy)
        {
            ChildWeight cw = new ChildWeight();

            cw.ChildId = childId;
            cw.Date = date;
            cw.Weight = weight;
            cw.ModifiedOn = modifiedOn;
            cw.ModifiedBy = modifiedBy;

            int i = ChildWeight.Insert(cw);
            IntReturnValue irv = new IntReturnValue();
            irv.id = i;
            return irv;
        }

        public IntReturnValue RegisterChildWeightBarcode(string barcode, DateTime date, double weight, DateTime modifiedOn, int modifiedBy)
        {
            GIIS.DataLayer.Child c = GIIS.DataLayer.Child.GetChildByBarcode(barcode);
            int i = -99;
            if (c != null)
            {
                int childId = GetActualChildId(c.Id);

                ChildWeight cw = ChildWeight.GetChildWeightByChildIdAndDate(childId, date);
                if (cw != null)
                {
                    cw.Weight = weight;
                    cw.ModifiedOn = modifiedOn;
                    cw.ModifiedBy = modifiedBy;
                    i = ChildWeight.Update(cw);
                }
                else
                {
                    cw = new ChildWeight();
                    cw.ChildId = childId;
                    cw.Date = date;
                    cw.Weight = weight;
                    cw.ModifiedOn = modifiedOn;
                    cw.ModifiedBy = modifiedBy;

                    i = ChildWeight.Insert(cw);
                }
            }
            IntReturnValue irv = new IntReturnValue();
            irv.id = i;
            return irv;
        }

        public List<Weight> GetWeight()
        {
            List<Weight> lw = Weight.GetWeightList();
            return lw;
        }

        public List<ChildEntity> GetChildById(int childId)
        {
            Child child = Child.GetChildById(childId);
            List<ChildEntity> ceList = new List<ChildEntity>();

            if (child != null)
            {
                List<GIIS.DataLayer.VaccinationEvent> veList = GIIS.DataLayer.VaccinationEvent.GetChildVaccinationEvent(child.Id);
                List<GIIS.DataLayer.VaccinationAppointment> vaList = GIIS.DataLayer.VaccinationAppointment.GetVaccinationAppointmentsByChild(child.Id);

                ChildEntity ce = new ChildEntity();
                ce.childEntity = child;
                ce.vaList = vaList; // GetVaccinationAppointment(vaList);
                ce.veList = veList; // GetVaccinationEvent(veList);
                ceList.Add(ce);
            }

            return ceList;

        }

        public ChildListEntity GetChildByIdList(string childIdList, int userId)
        {
            if (userId > 0)
            {
                User user = User.GetUserById(userId);
                ChildListEntity cle = new ChildListEntity();

                List<Child> childList = Child.GetChildByIdList(childIdList, userId);
                cle.childList = childList;

                List<GIIS.DataLayer.VaccinationAppointment> vaList = GIIS.DataLayer.VaccinationAppointment.GetVaccinationAppointmentsByChildBefore(childIdList, user.Lastlogin, user.PrevLogin, userId);
                cle.vaList = vaList;

                List<GIIS.DataLayer.VaccinationEvent> veList = GIIS.DataLayer.VaccinationEvent.GetChildVaccinationEventBefore(childIdList, user.Lastlogin, user.PrevLogin, user.Id);
                cle.veList = veList;
                return cle;
            }
            else
                return null;
            //string[] childList = childIdList.Split(',');
            //User user = User.GetUserById(userId);

            //List<ChildEntity> ceList = new List<ChildEntity>();

            //List<Child> childList = Child.GetChildByIdList(childIdList, userId);

            //foreach (Child child in childList)
            //{
            //    List<GIIS.DataLayer.VaccinationEvent> veList = GIIS.DataLayer.VaccinationEvent.GetChildVaccinationEventBefore(child.Id, user.Lastlogin, user.PrevLogin, user.Id);
            //    List<GIIS.DataLayer.VaccinationAppointment> vaList = GIIS.DataLayer.VaccinationAppointment.GetVaccinationAppointmentsByChildBefore(child.Id, user.Lastlogin, user.Id);

            //    ChildEntity ce = new ChildEntity();
            //    ce.childEntity = child;
            //    ce.vaList = vaList;// GetVaccinationAppointment(vaList);
            //    ce.veList = veList; // GetVaccinationEvent(veList);

            //    ceList.Add(ce);
            //}

            //return ceList;
        }

        public ChildListEntity GetChildByIdListSince(string childIdList, int userId)
        {
            ////string[] childList = childIdList.Split(',');
            //User user = User.GetUserById(userId);

            //List<ChildEntity> ceList = new List<ChildEntity>();

            //List<Child> childList = Child.GetChildByIdListSince(childIdList, userId);

            //foreach (Child child in childList)
            //{
            //    List<GIIS.DataLayer.VaccinationEvent> veList = GIIS.DataLayer.VaccinationEvent.GetChildVaccinationEvent(child.Id, user.Lastlogin, user.Id);
            //    List<GIIS.DataLayer.VaccinationAppointment> vaList = GIIS.DataLayer.VaccinationAppointment.GetVaccinationAppointmentsByChild(child.Id, user.Lastlogin, user.Id);

            //    ChildEntity ce = new ChildEntity();
            //    ce.childEntity = child;
            //    ce.vaList = vaList;// GetVaccinationAppointment(vaList);
            //    ce.veList = veList; // GetVaccinationEvent(veList);

            //    ceList.Add(ce);
            //}

            //return ceList;
            if (userId > 0)
            {
                User user = User.GetUserById(userId);
                ChildListEntity cle = new ChildListEntity();

                List<Child> childList = Child.GetChildByIdList(childIdList, userId);
                cle.childList = childList;

                List<GIIS.DataLayer.VaccinationAppointment> vaList = GIIS.DataLayer.VaccinationAppointment.GetVaccinationAppointmentsByChild(childIdList, user.Lastlogin, user.Id);
                cle.vaList = vaList;

                List<GIIS.DataLayer.VaccinationEvent> veList = GIIS.DataLayer.VaccinationEvent.GetChildVaccinationEvent(childIdList, user.Lastlogin, user.Id);
                cle.veList = veList;
                return cle;
            }
            else
                return null;
        }
        public List<ChildEntity> GetChildByBarcodeList(string childList)
        {

            List<ChildEntity> ceList = new List<ChildEntity>();
            List<Child> chList = new List<Child>();
            string[] cList = childList.Split(',');
            foreach (string s in cList)
            {
                Child c = Child.GetChildByBarcode(s);
                chList.Add(c);
            }
            foreach (Child child in chList)
            {
                if (child != null)
                {
                    List<GIIS.DataLayer.VaccinationEvent> veList = GIIS.DataLayer.VaccinationEvent.GetChildVaccinationEvent(child.Id);
                    List<GIIS.DataLayer.VaccinationAppointment> vaList = GIIS.DataLayer.VaccinationAppointment.GetVaccinationAppointmentsByChild(child.Id);

                    ChildEntity ce = new ChildEntity();
                    ce.childEntity = child;
                    ce.vaList = vaList;// GetVaccinationAppointment(vaList);
                    ce.veList = veList; // GetVaccinationEvent(veList);

                    ceList.Add(ce);
                }
            }

            return ceList;
        }

        private int GetActualChildId(int childId)
        {
            ChildMerges cm = ChildMerges.GetChildMergesBySubsumedId(childId);
            if (cm != null)
                return cm.ChildId;

            return childId;
        }
    }
}
