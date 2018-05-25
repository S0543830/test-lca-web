using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using LCA_WEB.Models;
using Microsoft.AspNet.Identity;
using PagedList;
using Newtonsoft.Json; // notwendig für die Diagramme
namespace LCA_WEB.Controllers
{
    public class HomeController : Controller
    {

        private DbWebModelEntities _db = new DbWebModelEntities();

        public ActionResult Index(string sortOn, string orderBy,
            string pSortOn, string keyword, int? page)
        {
            //if (Request.IsAuthenticated)
            {

                int recordsPerPage = 10;
                if (!page.HasValue)
                {
                    page = 1; // set initial page value
                    if (string.IsNullOrWhiteSpace(orderBy) || orderBy.Equals("asc"))
                    {
                        orderBy = "desc";
                    }
                    else
                    {
                        orderBy = "asc";
                    }
                }
                if (!string.IsNullOrWhiteSpace(sortOn) && !sortOn.Equals(pSortOn,
                        StringComparison.CurrentCultureIgnoreCase))
                {
                    orderBy = "asc";
                }

                ViewBag.OrderBy = orderBy;
                ViewBag.SortOn = sortOn;
                ViewBag.Keyword = keyword;

                var list = _db.Produkts.AsQueryable();

                switch (sortOn)
                {
                    case "Name":
                        if (orderBy.Equals("desc"))
                        {
                            list = list.OrderByDescending(p => p.Name);
                        }
                        else
                        {
                            list = list.OrderBy(p => p.Name);
                        }
                        break;
                    default:
                        list = list.OrderBy(p => p.Id);
                        break;
                }
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    list = list.Where(f => f.Name.StartsWith(keyword));
                }
                var finalList = list.ToPagedList(page.Value, recordsPerPage);
                return View(finalList);



                //int recordsPerPage = 10;
                //return View(_db.Produkts.ToList().ToPagedList(page, recordsPerPage));
            }
            //else
            //{
            //    return RedirectToAction("Login", "Account");
            //}

        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Imprint()
        {
            return View();
        }

        public ActionResult Help()
        {
            return View();
        }



        [HttpGet]
        public ActionResult Create(Produkt_Typ_Rohstoff_Indikator _produktTypRohstoffIndikator = null)
        {
            Produkt_Typ_Rohstoff_Indikator viewModel = new Produkt_Typ_Rohstoff_Indikator();
            ProduktRohstoff produktRohstoff = new ProduktRohstoff();
                produktRohstoff.LRohstoffe = new List<Umweltindikatorwert>();
                produktRohstoff.LRohstoffe.Add(new Umweltindikatorwert());
                if (_produktTypRohstoffIndikator != null)
                {
                    _produktTypRohstoffIndikator.ProduktRohstoff = new List<ProduktRohstoff>();
                    _produktTypRohstoffIndikator.ProduktRohstoff.Add(produktRohstoff);
                    viewModel = new Produkt_Typ_Rohstoff_Indikator
                    {
                        _Typ_Id = _db.ProduktTyps.ToList(),
                        _LIndikator = _db.Umweltindikators.ToList(),
                        _Rohstoffe = _db.Rohstoffes.ToList(),
                        ProduktRohstoff = _produktTypRohstoffIndikator.ProduktRohstoff,
                        _MengeEinheit = _db.MengeEinheits.ToList()
                    };
                }
            return View(viewModel);
        }

        [HttpPost,ActionName("Create")]
        [ValidateAntiForgeryToken]
        public ActionResult CreateConfirmed(Produkt_Typ_Rohstoff_Indikator _produktDetails)
        {
                try
                {
                    if (ModelState.IsValid)
                    {
                        _produktDetails._Produkt.DateOfChanging = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                        _produktDetails._Produkt.DateOfCreation = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                        _produktDetails._Produkt.CreatedBy = Request.IsAuthenticated ? User.Identity.GetUserName() : "Besucher";
                        _produktDetails._Produkt.ChangedBy = Request.IsAuthenticated ? User.Identity.GetUserName() : "Besucher";
                        _produktDetails._Produkt.Typ_Id = _produktDetails._ProduktTyp.Id;
                        _db.Produkts.Add(_produktDetails._Produkt);
                        _db.SaveChanges();
                        int id = _produktDetails._Produkt.Id;
                        //Indikator speichern
                      
                        for (int i = 0; i < _produktDetails.ProduktRohstoff.Count; i++)
                        {
                            _produktDetails.ProduktRohstoff[i].Rohstoff.Produkt_Id = id;
                        
                            _db.Rohstoffs.Add(_produktDetails.ProduktRohstoff[i].Rohstoff);
                            _db.SaveChanges();
                                
                                for (int j = 0; j < _produktDetails.ProduktRohstoff.ElementAt(i).LRohstoffe.Count; j++)
                                {
                                   _produktDetails.ProduktRohstoff.ElementAt(i).LRohstoffe.ElementAt(j).Rohstoff_Id =
                                   _produktDetails.ProduktRohstoff.ElementAt(i).Rohstoff.Id;
                                    _db.Umweltindikatorwerts.Add(_produktDetails.ProduktRohstoff.ElementAt(i).LRohstoffe
                                        .ElementAt(j));
                                    _db.SaveChanges();
                                }
                        
                           
                        }
                      
                        return RedirectToAction("Index");
                    }
                }
                catch (DbEntityValidationException dbEx)
                {
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            Trace.TraceInformation("Property: {0} Error: {1}",
                                validationError.PropertyName,
                                validationError.ErrorMessage);
                        }
                    }
                }
            
           
            return HttpNotFound();
        }

        

        [HttpGet]
        public ActionResult Edit(int? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Produkt_Typ_Rohstoff_Indikator produkt= new Produkt_Typ_Rohstoff_Indikator();
            List<ProduktRohstoff> lPRUW = new List<ProduktRohstoff>();
            ProduktRohstoff pr = new ProduktRohstoff();
            var roh = _db.Rohstoffs.Select(s => s).ToList();
            var uw = _db.Umweltindikatorwerts.Select(s => s).ToList();
            for (int i = 0; i < roh.Count; i++)
            {
                if (roh[i].Produkt_Id == Convert.ToInt32(Id))
                {
                    pr.Rohstoff = roh[i];
                    pr.LRohstoffe = new List<Umweltindikatorwert>();
                    for (int j = 0; j < uw.Count; j++)
                    {
                        if (roh[i].Id == uw[j].Rohstoff_Id)
                        {
                            pr.LRohstoffe.Add(uw[j]);       
                        }
                    }
                    lPRUW.Add(pr);
                }
                
            }
                produkt = new Produkt_Typ_Rohstoff_Indikator
                {
                    _Typ_Id = _db.ProduktTyps.ToList(),
                    _Produkt = _db.Produkts.Find(Id),
                    _LIndikator = _db.Umweltindikators.ToList(),
                    _Rohstoffe = _db.Rohstoffes.ToList(),
                    ProduktRohstoff = lPRUW,
                    _MengeEinheit = _db.MengeEinheits.ToList()
                };
                
                if (produkt._Produkt == null)
                {
                    return HttpNotFound();
                }
            return View(produkt);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit( Produkt_Typ_Rohstoff_Indikator _produktDetails)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _db.Entry(_produktDetails._Produkt).State = EntityState.Modified;
                    _produktDetails._Produkt.DateOfChanging = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                    _produktDetails._Produkt.ChangedBy = Request.IsAuthenticated ? User.Identity.GetUserName() : "Besucher";
                    for (int i = 0; i < _produktDetails.ProduktRohstoff.Count; i++)
                    {
                        _produktDetails.ProduktRohstoff[i].Rohstoff.Produkt_Id = _produktDetails._Produkt.Id;
                        _db.Rohstoffs.Add(_produktDetails.ProduktRohstoff[i].Rohstoff);
                        _db.SaveChanges();
                        for (int j = 0; j < _produktDetails.ProduktRohstoff.ElementAt(i).LRohstoffe.Count; j++)
                        {
                            _produktDetails.ProduktRohstoff[i].LRohstoffe[j].Rohstoff_Id =
                                _produktDetails.ProduktRohstoff[i].Rohstoff.Id;
                            _db.Umweltindikatorwerts.Add(_produktDetails.ProduktRohstoff[i].LRohstoffe[j]);
                            _db.SaveChanges();
                        }
                    }

                    //Indikator speichern
                    //   var indi = from a in _db.Umweltindikators where a.Id == _produktDetails._Indikator.Id select a.Id;
                    //   _produktDetails._Umweltindikatorwert.Umweltindikator_Id = indi.FirstOrDefault();
                    //   _produktDetails._Umweltindikatorwert.Wert = _produktDetails._Umweltindikatorwert.Wert;
                    //   _db.Umweltindikatorwerts.Add(_produktDetails._Umweltindikatorwert);
                    //   _db.SaveChanges();
                    //   var idindi = from a in _db.Umweltindikatorwerts
                    //       where (a.Umweltindikator_Id == indi.FirstOrDefault()) && (a.Wert == _produktDetails._Umweltindikatorwert.Wert)
                    //       select a.Id;
                    //   //Indikator speichern

                    //   //Rohstoff speichern
                    //  // _produktDetails._Rohstoff.Umweltindikator_Id = idindi.FirstOrDefault();
                    //   _produktDetails._Rohstoff.Rohstoff_Id = _produktDetails._Rohstoff.Id;
                    //   _db.Rohstoffs.Add(_produktDetails._Rohstoff);
                    //   _db.SaveChanges();
                    //   var rohid = from a in _db.Rohstoffs where (a.Rohstoff_Id == _produktDetails._Rohstoff.Id) && (a.Menge_in_t == _produktDetails._Rohstoff.Menge_in_t) select a.Id;
                    //   //Rohstoff speichern Ende

                    //   //Produkt speichern
                    //   var pa = from a in _db.ProduktTyps where a.Id == _produktDetails._ProduktTyp.Id select a.Id;
                    //   _produktDetails._Produkt.Typ_Id = pa.FirstOrDefault();
                    ////   _produktDetails._Produkt.Rohstoff_Id = rohid.FirstOrDefault();
                    //   _db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}",
                            validationError.PropertyName,
                            validationError.ErrorMessage);
                    }
                }
            }

            return View(_produktDetails);
        }

        [HttpGet()]
        public ActionResult Details(int? Id)
        {
            var proid = _db.Produkts.Find(Id);
            var ro = _db.Rohstoffs.Select(s => s).ToList();
            var uw = _db.Umweltindikatorwerts.Select(s => s).ToList();

            // var pri = _db.ProduktRohstoffUmweltindikators.Select(s => s).ToList();

            // var roh = _db.Rohstoffes.Select(s => s).ToList();//Finde alle Rohstoffe
            // var uwi = _db.Umweltindikators.Select(s => s).ToList();//Finde alle Umwelindikatoren

            List<Umweltindikatorwert> lUmweltind = new List<Umweltindikatorwert>();
            List<Rohstoff> lRohstoff = new List<Rohstoff>();
            foreach (var itemRo in ro)
            {
                if (proid != null && itemRo.Produkt_Id == proid.Id)
                {
                    lRohstoff.Add(itemRo);
                    foreach (var itemUw in uw)
                    {
                        if (itemRo.Id == itemUw.Rohstoff_Id)
                        {
                            lUmweltind.Add(itemUw);
                        }
                    }
                }
            }

            Produkt_Typ_Rohstoff_Indikator _view = new Produkt_Typ_Rohstoff_Indikator();
            _view._Produkt = proid;
            _view._ProduktTyp = _db.ProduktTyps.FirstOrDefault(i => i.Id == _view._Produkt.Typ_Id);
            _view._lRohstoff = lRohstoff;
            _view._Rohstoffe = _db.Rohstoffes.Select(s => s).ToList();
            _view._lUmweltindi = lUmweltind;
            _view._LIndikator = _db.Umweltindikators.Select(s => s).ToList();

            //Diagramm-Data
            List<DataPoint> dataPoints = new List<DataPoint>();
            foreach (var itemUw in _view._lUmweltindi)
            {
                dataPoints.Add(new DataPoint(_view._LIndikator.FirstOrDefault(i => i.Id == itemUw.Umweltindikator_Id).Name, itemUw.Wert));
            }

            ViewBag.DataPoints = (dataPoints);
            return View(_view);
        }

        //[HttpGet()]
        //public ActionResult Details(int? Id)
        //{
        //    if (Id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Produkt pro = _db.Produkts.Find(Id);
        //    ProduktTyp typ = new ProduktTyp();
        //    var produktTyp = _db.ProduktTyps.Select(s => s).ToList();
        //    foreach (var item in produktTyp)
        //    {
        //        if (pro != null && item.Id == pro.Typ_Id)
        //        {
        //            typ = item;
        //        }
        //    }

        //    var rohstoffe = _db.Rohstoffs.Select(s => s).ToList();
        //    var rohstoffeinheit = _db.Rohstoffes.Select(s => s).ToList();
        //    var mengeeinheit = _db.MengeEinheits.Select(s => s).ToList();
        //    List<CanvasDiagrammRohstoff> lCanvasRohstoff = new List<CanvasDiagrammRohstoff>();
        //    List<CanvasDiagrammRohstoff> lCanvasRohstofftemp = new List<CanvasDiagrammRohstoff>();
        //    CanvasDiagrammRohstoff sd = new CanvasDiagrammRohstoff();
        //    var umeinheit = _db.Umweltindikators.Select(s => s).ToList();
        //    var um = _db.Umweltindikatorwerts.Select(s => s).ToList();
        //    List<CanvasDiagrammIndikator> lCanvasDiagrammIndikators = new List<CanvasDiagrammIndikator>();
        //    List<CanvasDiagrammIndikator> lCanvasDiagrammIndikatorstemp = new List<CanvasDiagrammIndikator>();
        //    CanvasDiagrammIndikator sad = new CanvasDiagrammIndikator();
        //    double rohwert = 0;
        //    foreach (var item in rohstoffe)
        //    {
        //        if (item.Produkt_Id == pro.Id)
        //        {
        //            foreach (var lreinheit in rohstoffeinheit)
        //            {
        //                if (lreinheit.Id == item.Rohstoff_Id)
        //                {
        //                    if (0 != lCanvasRohstoff.Count)
        //                    {

        //                        for (int i = 0; i < lCanvasRohstoff.Count; i++)
        //                        {
        //                            foreach (var meinheit in mengeeinheit)
        //                            {
        //                                if (meinheit.Id == item.MengeEinheit_Id)
        //                                {
        //                                    if (meinheit.Name.Replace(" ", "") == "g")
        //                                    {
        //                                        rohwert = (double)(item.Menge * 1000000);
        //                                    }
        //                                    else if (meinheit.Name.Replace(" ", "") == "kg")
        //                                    {
        //                                        rohwert = (double)(item.Menge * 1000);
        //                                    }
        //                                    else
        //                                    {
        //                                        rohwert = (double)(item.Menge);
        //                                    }
        //                                }
        //                            }
        //                            if (lCanvasRohstoff[i].Name.Replace(" ", "") == lreinheit.Name.Replace(" ", ""))
        //                            {
        //                                lCanvasRohstoff[i].Wert = lCanvasRohstoff[i].Wert + rohwert;
        //                            }
        //                            else
        //                            {
        //                                sd.Wert = rohwert;
        //                                sd.Name = lreinheit.Name.Replace(" ", "");
        //                                lCanvasRohstofftemp.Add(sd);
        //                            }
        //                        }
        //                        for (int j = 0; j < lCanvasRohstofftemp.Count; j++)
        //                        {
        //                            lCanvasRohstoff.Add(lCanvasRohstofftemp[j]);
        //                            lCanvasRohstofftemp.Clear();
        //                        }
        //                    }
        //                    else
        //                    {
        //                        foreach (var meinheit in mengeeinheit)
        //                        {
        //                            if (meinheit.Id == item.MengeEinheit_Id)
        //                            {
        //                                if (meinheit.Name.Replace(" ", "") == "g")
        //                                {
        //                                    if (null != item.Menge)
        //                                    {
        //                                        rohwert = (double)(item.Menge * 1000000);
        //                                    }

        //                                }
        //                                else if (meinheit.Name.Replace(" ", "") == "kg")
        //                                {
        //                                    if (null != item.Menge)
        //                                    {
        //                                        rohwert = (double)(item.Menge * 1000);
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    if (null != item.Menge)
        //                                    {
        //                                        rohwert = (double)(item.Menge);
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        sd.Wert = rohwert;
        //                        sd.Name = lreinheit.Name.Replace(" ", "");
        //                        lCanvasRohstoff.Add(sd);
        //                    }
        //                    foreach (var umItem in um)
        //                    {
        //                        if (umItem.Rohstoff_Id == item.Id)
        //                        {
        //                            foreach (var umEinheitItem in umeinheit)
        //                            {
        //                                if (umEinheitItem.Id == umItem.Umweltindikator_Id)
        //                                {
        //                                    if (0 != lCanvasDiagrammIndikators.Count)
        //                                    {

        //                                        for(int i = 0;i< lCanvasDiagrammIndikators.Count;i++)
        //                                        {
        //                                            if (lCanvasDiagrammIndikators[i].Name.Replace(" ", "") == umEinheitItem.Name.Replace(" ", ""))
        //                                            {
        //                                                lCanvasDiagrammIndikators[i].SummeWert =
        //                                                    (double) (lCanvasDiagrammIndikators[i].SummeWert + umItem.Wert);
        //                                            }
        //                                            else
        //                                            {
        //                                                sad.SummeWert = (double) umItem.Wert;
        //                                                sad.Name = umEinheitItem.Name.Replace(" ", "");
        //                                                lCanvasDiagrammIndikatorstemp.Add(sad);
        //                                            }
        //                                        }
        //                                        for (int j = 0; j < lCanvasDiagrammIndikatorstemp.Count; j++)
        //                                        {
        //                                            lCanvasDiagrammIndikators.Add(lCanvasDiagrammIndikatorstemp[j]);
        //                                            lCanvasDiagrammIndikatorstemp.Clear();
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        sad.SummeWert = (double) umItem.Wert;
        //                                        sad.Name = umEinheitItem.Name.Replace(" ","");
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //    }




        //    CanvasDiagramm diagramm = new CanvasDiagramm
        //    {
        //        CanvasRohstoff = lCanvasRohstoff,
        //        CanvasIndikator = lCanvasDiagrammIndikators,
        //        Produkt = pro,
        //        _ProduktTyp = typ
        //    };

        //    ViewBag.Rohstoff = lCanvasRohstoff;
        //    ViewBag.Indikator = lCanvasDiagrammIndikators;
        //    return View(diagramm);
        //}

        [HttpGet]
        public ActionResult AddIndikator(string whichView)
        {
            IndikatorViewViewModel sd = new IndikatorViewViewModel();
            sd._View = whichView;
            return View(sd);
        }

        [HttpGet]
        public ActionResult AddRohstoff(string whichView)
        {
            RohstoffViewViewModel r = new RohstoffViewViewModel();
            r._View = whichView;
            return View(r);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AddRohstoff(RohstoffViewViewModel _rohstoffView)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _db.Rohstoffes.Add(_rohstoffView._Rohstoffe);
                    _db.SaveChanges();
                    return RedirectToAction(_rohstoffView._View);
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}",
                            validationError.PropertyName,
                            validationError.ErrorMessage);
                    }
                }
            }
            return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteRohstoff(Rohstoffe _name, string roh)
        {
            if (_name == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RohstoffViewViewModel sd = new RohstoffViewViewModel();
            _name.Id = Convert.ToInt32(_name.Name);
            sd._Rohstoffe = _db.Rohstoffes.FirstOrDefault(i => i.Id == _name.Id);
            sd._View = roh;
            if (sd._Rohstoffe == null)
            {
                return HttpNotFound();
            }
            return View(sd);
        }

        [HttpPost, ActionName("DeleteRohstoff")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRohstoffConfirmed(Rohstoffe _name, string roh)
        {
            _name.Id = Convert.ToInt32(_name.Name);
            var p = from a in _db.Rohstoffes where a.Id == _name.Id select a;
            Rohstoffe asd = p.FirstOrDefault();//_db.Rohstoffes.FirstOrDefault(i => i.Id == _name.Id);
            if (asd != null)
            {
                //Alle Beziehungen mit dem Rohstoff löschen
                var li = _db.Rohstoffs.Select(s => s).ToList(); //Finde alle Rohstoffe
                var end = _db.EndOfLifeDatas.Select(s => s).ToList(); //EndofLifeData
                var pri = _db.ProduktRohstoffUmweltindikators.Select(s => s).ToList();
                var iw = _db.Umweltindikatorwerts.Select(s => s).ToList();
                //Rohstoff, EndOfLifeData , ProduktRohstoffIndikator löschen

                //Rohstoff löschen
                for (int i = 0; i < li.Count(); i++)
                {
                    if (li.ElementAt(i).Rohstoff_Id == asd.Id)
                    {
                        //Lösche EndOfLifeData
                        for (int j = 0; j < end.Count(); j++)
                        {
                            if (end.ElementAt(j).Rohstoff_Id == li.ElementAt(i).Rohstoff_Id)
                            {
                                //Lösche ProduktRohstoffIndikator
                                for (int z = 0; z < pri.Count(); z++)
                                {
                                    if (pri.ElementAt(z).Rohstoff_Id == end.ElementAt(j).Rohstoff_Id)
                                    {
                                        _db.ProduktRohstoffUmweltindikators.Remove(pri.ElementAt(z));
                                    }
                                }
                                _db.EndOfLifeDatas.Remove(end.ElementAt(j));
                            }
                        }
                        _db.Rohstoffs.Remove(li.ElementAt(i));
                    }
                }
                for (int i = 0; i < iw.Count(); i++)
                {
                    if (iw.ElementAt(i).Rohstoff_Id == asd.Id)
                    {
                        _db.Entry(iw.ElementAt(i)).State = EntityState.Modified;
                        iw.ElementAt(i).Rohstoff_Id = null;
                    }
                }
                _db.Rohstoffes.Remove(asd);
            }
            
            _db.SaveChanges();
            return RedirectToAction(roh);
        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AddIndikator(IndikatorViewViewModel _indiDetails)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _db.Umweltindikators.Add(_indiDetails._Umweltindikator);
                    _db.SaveChanges();
                    return RedirectToAction(_indiDetails._View);
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}",
                            validationError.PropertyName,
                            validationError.ErrorMessage);
                    }
                }
            }
            return HttpNotFound();
        }

        [HttpGet]
        public ActionResult DeleteIndikator(Umweltindikator _name, string indi)
        {

            if (_name == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            IndikatorViewViewModel sd = new IndikatorViewViewModel();
            _name.Id = Convert.ToInt32(_name.Name);
            sd._Umweltindikator = _db.Umweltindikators.FirstOrDefault(i => i.Id == _name.Id);
            sd._View = indi;
            if (sd._Umweltindikator == null)
            {
                return HttpNotFound();
            }
            return View(sd);
        }

        [HttpPost, ActionName("DeleteIndikator")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteIndikatorConfirmed(Umweltindikator _name, string indi)
        {
            _name.Id = Convert.ToInt32(_name.Name);
            var pa = from a in _db.Umweltindikators where a.Id == _name.Id select a;
            Umweltindikator asdd = pa.FirstOrDefault();
            if (asdd != null)
            {
                //Finde alle Umweltindikator
                var uw = _db.Umweltindikatorwerts.Select(s => s).ToList();
                for (int i = 0; i < uw.Count(); i++)
                {
                    if (uw.ElementAt(i).Umweltindikator_Id == _name.Id)
                    {
                         _db.Umweltindikatorwerts.Remove(uw.ElementAt(i));
                    }
                }
                _db.Umweltindikators.Remove(asdd);
            }
            _db.SaveChanges();
            
            return RedirectToAction(indi);
           
        }

        [HttpGet]
        public ActionResult DeleteProduktTyp(ProduktTyp _name, string _whichView)
        {
            if (_name == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            _name.Id = Convert.ToInt32(_name.Name);
            ProduktTyp viewModel = new ProduktTyp();
            viewModel = _db.ProduktTyps.FirstOrDefault(i => i.Id == _name.Id);
            ProduktTypViewViewModel _deleteProduktTypView = new ProduktTypViewViewModel();
            _deleteProduktTypView._ProduktTyp = viewModel;
            _deleteProduktTypView._View = _whichView;
            if (viewModel == null)
            {
                return HttpNotFound();
            }
            return View(_deleteProduktTypView);
        }

        [HttpPost, ActionName("DeleteProduktTyp")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteTypConfirmed(ProduktTyp _name, string _whichView)
        {
            _name.Id = Convert.ToInt32(_name.Name);
            var p = from a in _db.ProduktTyps where a.Id == _name.Id select a;
            var pr = _db.Produkts.Select(s => s).ToList();
            ProduktTyp asd = p.FirstOrDefault();
            if (asd != null)
            {
                for (int i = 0; i < pr.Count(); i++)
                {
                    if (pr.ElementAt(i).Typ_Id == _name.Id)
                    {
                        _db.Entry(pr.ElementAt(i)).State = EntityState.Modified;
                        pr.ElementAt(i).Typ_Id = null;
                    }
                }
                _db.ProduktTyps.Remove(asd);
            }
            _db.SaveChanges();
            return RedirectToAction(_whichView);
        }

        
        public ActionResult AddProduktTyp(string whichView)
        {
            ProduktTypViewViewModel v = new ProduktTypViewViewModel();
            v._View = whichView;
            return View(v);
        }

        [HttpPost, ActionName("AddProduktTyp")]
        [ValidateAntiForgeryToken]
        public ActionResult AddProduktTypConfirmed(ProduktTypViewViewModel _typ)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _typ._ProduktTyp.Typ = _typ._ProduktTyp.Name;
                    _db.ProduktTyps.Add(_typ._ProduktTyp);
                    _db.SaveChanges();
                    return RedirectToAction(_typ._View);
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}",
                            validationError.PropertyName,
                            validationError.ErrorMessage);
                    }
                }
            }
            return HttpNotFound();
        }
        
        [HttpGet()]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Produkt_Typ_Rohstoff_Indikator viewModel = new Produkt_Typ_Rohstoff_Indikator
            {
                _Typ_Id = _db.ProduktTyps.ToList(),
                _LIndikator = _db.Umweltindikators.ToList(),
                _Produkt = _db.Produkts.Find(id)
        };
            if (viewModel._Produkt == null)
            {
                return HttpNotFound();
            }
            return View(viewModel._Produkt);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Produkt produkt = _db.Produkts.Find(id);
            if (produkt != null)
            {
                _db.Produkts.Remove(produkt);
            }
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

    }
}