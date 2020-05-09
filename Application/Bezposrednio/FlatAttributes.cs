using HtmlAgilityPack;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Application.Bezposrednio
{
    class FlatAttributes
    {

        private HtmlDocument docWebPage;
        private string url;

        public FlatAttributes(HtmlDocument docWebPage, string url)
        {
            this.docWebPage = docWebPage;
            this.url = url;
        }


        private bool findInDecription(string keyValue)
        {
            HtmlNode node = docWebPage.DocumentNode.SelectSingleNode("//div[@class='property__description-wrap']");

            if (node == null)
            {
                return false;
            }
            else
            {
                string description = node.InnerText.ToUpper();

                if (description.Contains(keyValue.ToUpper()))
                {
                    return true;
                }
            }

            return false;

        }

        private string findEmailInDescription()
        {
            HtmlNode node = docWebPage.DocumentNode.SelectSingleNode("//div[@class='property__description-wrap']");

            if (node == null)
            {
                return null;
            }
            else
            {
                string textToScrape = node.InnerText;

                if (textToScrape.Contains("@"))
                {
                    string pattern = @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";

                    var m = Regex.Match(textToScrape, pattern);
                    if (m.Success)
                    {
                        return m.Value;
                    }
                }
            }

            return null;

        }

        private string replacePolishDiactricMarks(string word)
        {
            string newWord = word.Replace('Ą', 'A');
            newWord = newWord.Replace('Ć', 'C');
            newWord = newWord.Replace('Ę', 'E');
            newWord = newWord.Replace('Ł', 'L');
            newWord = newWord.Replace('Ń', 'N');
            newWord = newWord.Replace('Ó', 'O');
            newWord = newWord.Replace('Ś', 'S');
            newWord = newWord.Replace('Ż', 'Z');
            newWord = newWord.Replace('Ź', 'Z');
            newWord = newWord.Replace('ą', 'a');
            newWord = newWord.Replace('ć', 'c');
            newWord = newWord.Replace('ę', 'e');
            newWord = newWord.Replace('ł', 'l');
            newWord = newWord.Replace('ń', 'n');
            newWord = newWord.Replace('ó', 'o');
            newWord = newWord.Replace('ś', 's');
            newWord = newWord.Replace('ż', 'z');
            newWord = newWord.Replace('ź', 'z');

            return newWord;
        }

        public string Url
        {
            get
            {
                return url;
            }
        }

        public DateTime CreationDateTime   //<---Musiałem zmienić typ na string (zazwyczaj zamiast daty pojawia się lakoniczne "ponad 14, 30 dni temu")
        {
            get
            {
                return DateTime.Now;
            }
        }

        public OfferKind OfferKind
        {
            get
            {
                return OfferKind.SALE;
            }
        }

        public string Email   //    <----zazwyczaj brak emaila (na 100 ogłoszeń, tylko 3 emaile ;/)
        {
            get
            {
                string email = findEmailInDescription();
                if(email != null)
                {
                    return email;
                }

                return "";
            }
        }
        public string Telephone    // <---telefon tylko jako <img> (straszny bezsens ;p ) lub w opisie(ze względu na swoistość zachowania w opisie podaję tylko informację, że prawdopodobnie numer telefonu jest w opisie)
        {
            get
            {
                bool isEmail = findInDecription("tel");
                if (isEmail)
                {
                    return "Check the description, please";
                }

                return "";
            }
        }
        public string Name     // Brak właściciela, kontaktu do sprzedawcy
        {
            get
            {
                bool isEmail = findInDecription("mail");
                if (isEmail)
                {
                    return "Check the description, please";
                }

                return "";
            }
        }

        public bool IsStillValid  //<---  Skoro 'wisi' na stronie, to aktualne :)
        {
            get
            {
                return true;
            }
        }


        public int TotalGrossPrice
        {
            get
            {
                HtmlNode node = docWebPage.DocumentNode.SelectSingleNode("//strong[@class='property__price-value']");
                
                if(node != null)
                {
                    string priceText = node.InnerText;
                    string result = Regex.Replace(priceText, @"[^\d]", "");
                    int priceDec = Int32.Parse(result);

                    return priceDec;
                }

                return 0;

            }
        }

        public int PricePerMeter
        {
            get
            {
                HtmlNode node = docWebPage.DocumentNode.SelectSingleNode("//span[@class='property__price-label']");

                if (node != null)
                {
                    string priceText = node.InnerText;
                    string result = Regex.Replace(priceText, @"^.* : ", "");
                    result = Regex.Replace(result, @"[^\d]", "");

                    int priceDec = Int32.Parse(result);

                    return priceDec;
                }

                return 0;

            }
        }

        public int? ResidentalRent  // <----------- Czasami występują 'koszta pozalicznikowe', prawdopodobnie czynsz :)
        {
            get
            {
                HtmlNodeCollection nodes = docWebPage.DocumentNode.SelectNodes("//ul[@class='property__params-list']");
                string residentalRentText;
                if (nodes != null)
                {
                    foreach(HtmlNode node in nodes)
                    {

                        foreach(var li in node.ChildNodes)
                        {
                            residentalRentText = li.InnerText.ToLower();

                            if (residentalRentText.Contains("koszty pozalicznikowe"))
                            {
                                residentalRentText = Regex.Replace(residentalRentText, @"[^\d]", "");

                                int residentalRentInt = 0;
                                if (Int32.TryParse(residentalRentText, out residentalRentInt))
                                {
                                    return residentalRentInt;
                                }
                                return 0;
                            }
                        }
                    }
                }
                return null;
            }
        }

        public decimal Area
        {
            get
            {
                HtmlNode node = docWebPage.DocumentNode.SelectSingleNode("//h4[@class='property__commision']");
                string areaText;
                if (node != null)
                {
                    areaText = node.InnerText;
                    areaText = Regex.Replace(areaText, @"[^\d]", "");
                    areaText = areaText.Remove(areaText.Length - 1);

                    return Decimal.Parse(areaText);
                }
                return 0;
            }
        }

        public int NumberOfRooms
        {
            get
            {
                HtmlNodeCollection nodes = docWebPage.DocumentNode.SelectNodes("//ul[@class='property__params-list']");
                string numberOfRoomsText;
                if (nodes != null)
                {
                    foreach (HtmlNode node in nodes)
                    {

                        foreach (var li in node.ChildNodes)
                        {
                            numberOfRoomsText = li.InnerText.ToLower();

                            if (numberOfRoomsText.Contains("liczba pokoi"))
                            {
                                numberOfRoomsText = Regex.Replace(numberOfRoomsText, @"[^\d]", "");

                                int numberOfRoomsInt = 0;
                                if (Int32.TryParse(numberOfRoomsText, out numberOfRoomsInt))
                                {
                                    return numberOfRoomsInt;
                                }
                                return 0;
                            }
                        }
                    }
                }

                return 0;
            }
        }

        public int? FloorNumber
        {
            get
            {
                HtmlNodeCollection nodes = docWebPage.DocumentNode.SelectNodes("//ul[@class='property__params-list']");
                string floorNumberText;
                if (nodes != null)
                {
                    foreach (HtmlNode node in nodes)
                    {

                        foreach (var li in node.ChildNodes)
                        {
                            floorNumberText = li.InnerText.ToLower();

                            if (floorNumberText.Contains("piętro"))
                            {
                                floorNumberText = Regex.Replace(floorNumberText, @"[^\d]", "");
                                int floorNumberInt = 0;
                                if (Int32.TryParse(floorNumberText, out floorNumberInt))
                                {
                                    return floorNumberInt;
                                }
                                return 0;
                            }
                        }
                    }
                }

                return 0;
            }
        }



        public int? YearOfConstruction
        {
            get
            {
                HtmlNodeCollection nodes = docWebPage.DocumentNode.SelectNodes("//ul[@class='property__params-list']");
                string yearOfConstructionText;
                if (nodes != null)
                {
                    foreach (HtmlNode node in nodes)
                    {

                        foreach (var li in node.ChildNodes)
                        {
                            yearOfConstructionText = li.InnerText.ToLower();

                            if (yearOfConstructionText.Contains("rok budowy"))
                            {
                                yearOfConstructionText = Regex.Replace(yearOfConstructionText, @"[^\d]", "");
                                int yearOfConstructionInt = 0;
                                if (Int32.TryParse(yearOfConstructionText, out yearOfConstructionInt))
                                {
                                    return yearOfConstructionInt;
                                }
                                return 0;
                            }
                        }
                    }
                }

                return 0;
            }
        }

        public PolishCity City     //<----niewymagane, jeśłi miejscowość nieznana  lub nierozpoznana
        {
            get
            {
                HtmlNodeCollection nodes = docWebPage.DocumentNode.SelectNodes("//ul[@class='property__params-list']");
                string cityText;
                if (nodes != null)
                {
                    foreach (HtmlNode node in nodes)
                    {

                        foreach (var li in node.ChildNodes)
                        {
                            cityText = li.InnerText.ToLower();

                            if (cityText.Contains("miejscowość"))
                            {
                                cityText = Regex.Replace(cityText, @"^.*: ", "");
                                cityText = cityText.ToUpper();
                                cityText = cityText.Trim();
                                cityText = cityText.Replace(' ', '_');
                                cityText = replacePolishDiactricMarks(cityText).ToUpper();
                                PolishCity city;

                                if (Enum.TryParse(cityText, out city))
                                {
                                    return city;
                                }
                                return 0;
                            }
                        }
                    }
                }

                return 0;
            }
        }

        public string District
        {
            get
            {
                HtmlNodeCollection nodes = docWebPage.DocumentNode.SelectNodes("//ul[@class='property__params-list']");
                string districtText;
                if (nodes != null)
                {
                    foreach (HtmlNode node in nodes)
                    {

                        foreach (var li in node.ChildNodes)
                        {
                            districtText = li.InnerText.ToLower();

                            if (districtText.Contains("dzielnica"))
                            {
                                districtText = replacePolishDiactricMarks(Regex.Replace(districtText, @"^.*: ", ""));
                                return districtText;
                            }
                        }
                    }
                }
                return "";
            }
        }

        public string StreetName
        {
            get
            {
                HtmlNodeCollection nodes = docWebPage.DocumentNode.SelectNodes("//ul[@class='property__params-list']");
                string streetText;
                if (nodes != null)
                {
                    foreach (HtmlNode node in nodes)
                    {

                        foreach (var li in node.ChildNodes)
                        {
                            streetText = li.InnerText.ToLower();

                            if (streetText.Contains("ulica"))
                            {
                                streetText = replacePolishDiactricMarks(Regex.Replace(streetText, @"^.*: ", ""));
                                return streetText;
                            }
                        }
                    }
                }
                return "";
            }
        }

        public decimal? GardenArea //<---ma sens tylko w przypadków domów. Wtedy mozna określić powierzchnię działki. Informacje o ewnetualnym ogródku w bloku mozna wyciągnąć z opisu, ale jako informację czy taka jest, a nie o jej powierzchni
        {
            get
            {
                HtmlNodeCollection nodes = docWebPage.DocumentNode.SelectNodes("//ul[@class='property__params-list']");
                string gardenAreaText;
                if (nodes != null)
                {
                    foreach (HtmlNode node in nodes)
                    {

                        foreach (var li in node.ChildNodes)
                        {
                            gardenAreaText = li.InnerText.ToLower();

                            if (gardenAreaText.Contains("powierzchnia działki"))
                            {
                                gardenAreaText = Regex.Replace(gardenAreaText, @"^.*: ", "");
                                decimal gardenAreaDec = 0;
                                if (Decimal.TryParse(gardenAreaText, out gardenAreaDec))
                                {
                                    return gardenAreaDec;
                                }
                                return 0;
                            }
                        }
                    }
                    return 0;
                }
                return null;
            }
        }
        public int? Balconies
        {
            get
            {
                if (findInDecription("balkon"))
                {
                    return 1;
                }
                else
                {
                    HtmlNodeCollection nodes = docWebPage.DocumentNode.SelectNodes("//ul[@class='property__params-list property__params-list--options']");
                    string additionaInformation;
                    if (nodes != null)
                    {
                        foreach (HtmlNode node in nodes)
                        {

                            foreach (var li in node.ChildNodes)
                            {
                                additionaInformation = li.InnerText.ToLower();
                                if (additionaInformation.Contains("balkon"))
                                {
                                    return 1;
                                }
                            }
                        }
                        return 0;
                    }
                    return null;
                }
            }
        }
        public decimal? BasementArea  // <---sytuacja podobna jak z balkonem...return 1 oznacza,ze jest piwnica (trudno wydobyć dane o powierzchni)
        {
            get
            {
                if (findInDecription("piwnica"))
                {
                    return 1;
                }
                else
                {
                    HtmlNodeCollection nodes = docWebPage.DocumentNode.SelectNodes("//ul[@class='property__params-list property__params-list--options']");
                    string additionaInformation;
                    if (nodes != null)
                    {
                        foreach (HtmlNode node in nodes)
                        {

                            foreach (var li in node.ChildNodes)
                            {
                                additionaInformation = li.InnerText.ToLower();
                                if (additionaInformation.Contains("piwnica"))
                                {
                                    return 1;
                                }
                            }
                        }
                        return 0;
                    }
                    return null;
                }
            }
        }
        public int? OutdoorParkingPlaces // <---jedyna forma informacji to parkowanie (niezależnie czy dom, czy moieszkanie), dlatego uzupełniam tylko jedną informację o parkingu (w dodatku jako string)
        {
            get
            {
                HtmlNodeCollection nodes = docWebPage.DocumentNode.SelectNodes("//ul[@class='property__params-list']");
                string additionaInformation;
                if (nodes != null)
                {
                    foreach (HtmlNode node in nodes)
                    {

                        foreach (var li in node.ChildNodes)
                        {
                            additionaInformation = li.InnerText.ToLower();

                            if (additionaInformation.Contains("parkowanie"))
                            {
                                additionaInformation = replacePolishDiactricMarks(Regex.Replace(additionaInformation, @"^.*: ", ""));
                                return 1;
                            }
                        }
                    }
                }
                return 0;
            }
        }

        public string RawDescription
        {
            get
            {
                HtmlNode node = docWebPage.DocumentNode.SelectSingleNode("//div[@class='property__description-wrap']");
                string rawDescription; ;
                if (node != null)
                {
                    rawDescription = node.InnerText;
                    rawDescription = replacePolishDiactricMarks(rawDescription);
                    rawDescription = rawDescription.Replace("\r\n", string.Empty);
                    rawDescription = rawDescription.Trim();

                    return rawDescription;
                }
                return null;
            }
        }
    }
}
