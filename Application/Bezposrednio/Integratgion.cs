using Interfaces;
using Models;
using System;
using HtmlAgilityPack;
using System.Collections.Generic;
using Application.Bezposrednio;
using System.IO;

namespace Application.OfertyDom
{
    public class Integratgion : IWebSiteIntegration
    {
        public WebPage WebPage { get; }
        public IDumpsRepository DumpsRepository { get; }

        public IEqualityComparer<Entry> EntriesComparer { get; }

        public Integratgion(IDumpsRepository dumpsRepository,
            IEqualityComparer<Entry> equalityComparer)
        {
            DumpsRepository = dumpsRepository;
            EntriesComparer = equalityComparer;
            WebPage = new WebPage
            {
                Url = "https://bezposrednio.net.pl/",
                Name = "Bezposrednio WebSite Integration",
                WebPageFeatures = new WebPageFeatures
                {
                    HomeSale = true,
                    HomeRental = true,
                    HouseSale = true,
                    HouseRental = true
                }
            };
        }

        public Dump GenerateDump()
        {
            List<Entry> entries = GetFlatForSalesEntries();

            cleanImagesDirectory();

            return new Dump
            {
                DateTime = DateTime.Now,
                WebPage = WebPage,
                Entries = entries,
            };
        }

        public static List<Entry> GetEntryByPage(int pageNumber)
        {
            List<Entry> flatList = new List<Entry>();
            Entry flatDetail = new Entry();
            List<string> urls = collectUrlPerSite(pageNumber);


            foreach (var url in urls)
            {
                flatDetail = getEntryForFlat(url);
                flatList.Add(flatDetail);

            }


            return flatList;
        }

        public static List<Entry> GetFlatForSalesEntries()
        {
            List<Entry> flatList = new List<Entry>();
            Entry flatDetail = new Entry();
            List<string> urls = collectAllUrls();


            foreach(var url in urls)
            {
                flatDetail = getEntryForFlat(url);
                flatList.Add(flatDetail);

            }


            return flatList;
        }

        private static Entry getEntryForFlat(string url)
        {
            string url_base = "https://bezposrednio.net.pl/";
            string offerUrl = $"{url_base}{url}";
            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument doc = htmlWeb.Load(offerUrl);
            FlatAttributes flatAttributes = new FlatAttributes(doc, offerUrl);

            Entry flatDetails = new Entry()
            {
                OfferDetails = getOfferDetails(flatAttributes),
                PropertyPrice = getPropertyPrice(flatAttributes),
                PropertyDetails = getPropertyDetails(flatAttributes),
                PropertyAddress = getPropertyAddress(flatAttributes),
                PropertyFeatures = getPropertyFeatures(flatAttributes),
                RawDescription = getRawDescription(flatAttributes),

            };

            return flatDetails;
        }

        private static OfferDetails getOfferDetails(FlatAttributes flatAttributes)
        {

            OfferDetails offerDetails = new OfferDetails()
            {
                Url = flatAttributes.Url,
                CreationDateTime = flatAttributes.CreationDateTime,
                OfferKind = flatAttributes.OfferKind,
                SellerContact = new SellerContact
                {
                    Email = flatAttributes.Email,
                    Telephone = flatAttributes.Telephone,
                    Name = flatAttributes.Name,
                },
                IsStillValid = flatAttributes.IsStillValid,

            };

            return offerDetails;        
        }

        private static PropertyPrice getPropertyPrice(FlatAttributes flatAttributes)
        {
            PropertyPrice propertyPrice = new PropertyPrice()
            {
                TotalGrossPrice = flatAttributes.TotalGrossPrice,
                PricePerMeter = flatAttributes.PricePerMeter,
                ResidentalRent = flatAttributes.ResidentalRent,
            };
            return propertyPrice;
        }

        private static PropertyDetails getPropertyDetails(FlatAttributes flatAttributes)
        {
            PropertyDetails propertyDetails = new PropertyDetails()
            {
                Area = flatAttributes.Area,
                NumberOfRooms = flatAttributes.NumberOfRooms,
                FloorNumber = flatAttributes.FloorNumber,
                YearOfConstruction = flatAttributes.YearOfConstruction,
            };
            return propertyDetails;
        }

        private static PropertyAddress getPropertyAddress(FlatAttributes flatAttributes)
        {
            PropertyAddress propertyAddress = new PropertyAddress()
            {
                City = flatAttributes.City,
                District = flatAttributes.District,
                StreetName = flatAttributes.StreetName,
            };
            return propertyAddress;
        }

        private static PropertyFeatures getPropertyFeatures(FlatAttributes flatAttributes)
        {
            PropertyFeatures propertyFeatures = new PropertyFeatures()
            {
                GardenArea = flatAttributes.GardenArea,
                Balconies = flatAttributes.Balconies,
                BasementArea = flatAttributes.BasementArea,
                OutdoorParkingPlaces = flatAttributes.OutdoorParkingPlaces,
            };
            return propertyFeatures;
        }

        private static string getRawDescription(FlatAttributes flatAttributes)
        {
            string rawDescription = flatAttributes.RawDescription;
            return rawDescription;
        }

        private static List<string> collectUrlPerSite(int page)
        {
            List<string> flatUrlList = new List<string>();


            string url = $"https://bezposrednio.net.pl/?site=szukaj&kategoria=2&typ_trans_id=1&miejscowosc=warszawa&strona={page}";
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            addSalesAdvURL(doc, flatUrlList);


            return flatUrlList;
        }

        private static List<string> collectAllUrls()
        {
            List<string> flatUrlList = new List<string>();
            int counter = 1;

            while (true)
            {
                string url = $"https://bezposrednio.net.pl/?site=szukaj&kategoria=2&typ_trans_id=1&miejscowosc=warszawa&strona={counter}";
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(url);
                bool isMoreAdv = addSalesAdvURL(doc, flatUrlList);
                counter++;

                if (!isMoreAdv)
                {
                    int count = 1;
                    foreach (var urlt in flatUrlList)
                    {
                        count++;
                    }
                    break;
                }

            }

            return flatUrlList;
        }

        private static bool addSalesAdvURL(HtmlDocument doc, List<string> flatUrlList)
        {
            HtmlNodeCollection hrefs = doc.DocumentNode.SelectNodes("//a[@class='properties__address']");

            if (hrefs == null)
            {
                return false;
            }

            foreach (var href in hrefs)
            {
                flatUrlList.Add(href.GetAttributeValue("href", string.Empty));
            }

            return true;
        }

        private void cleanImagesDirectory()
        {
            string path = $"Bezposrednio/images/"; ;
            System.IO.DirectoryInfo di = new DirectoryInfo(path);

            foreach (FileInfo file in di.GetFiles())
            {
                try
                {
                    file.Delete();
                }
                catch (Exception)
                {
                }
            }

        }
    }
}
