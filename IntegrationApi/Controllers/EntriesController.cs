using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DatabaseConnection;
using Application.OfertyDom;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Models;
using Microsoft.EntityFrameworkCore.Internal;
using System.IO;

namespace IntegrationApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EntriesController : ControllerBase
    {
        private DatabaseContext dbContext;
        public EntriesController()
        {
            dbContext = new DatabaseContext();
        }

        ~EntriesController()
        {
            dbContext.Dispose();
        }

        [HttpGet]
        public IActionResult Get()
        {
            var entries = dbContext.Entries.Include(x => x.PropertyPrice)
                .Include(x => x.PropertyFeatures)
                .Include(x => x.PropertyDetails)
                .Include(x => x.PropertyAddress)
                .Include(x => x.OfferDetails).ThenInclude(x => x.SellerContact);


            if (entries.Count() == 0)
            {
                return new NotFoundResult();
            }

            return Ok(entries);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {

            if (id < 0 )
            {
                return new BadRequestResult();
            }

            var entries = dbContext.Entries.Include(x => x.PropertyPrice)
                .Include(x => x.PropertyFeatures)
                .Include(x => x.PropertyDetails)
                .Include(x => x.PropertyAddress)
                .Include(x => x.OfferDetails).ThenInclude(x => x.SellerContact).Where(e => e.ID == id);

            if (entries.Count() == 0)
            {
                return new NotFoundResult();
            }

            return Ok(entries);
        }

        [HttpGet("{pagelimit}_{pageid}")]
        public IActionResult Get(int PageLimit, int PageId)
        {
            Console.WriteLine(PageLimit +  " " + PageId);
            if (PageId < 0 || PageLimit<0)
            {
                return new BadRequestResult();
            }

            int start = (PageId - 1) * PageLimit;
            int end = PageId * PageLimit + 1;

            Console.WriteLine(start + " " + end);


            var entries = dbContext.Entries.Include(x => x.PropertyPrice)
                .Include(x => x.PropertyFeatures)
                .Include(x => x.PropertyDetails)
                .Include(x => x.PropertyAddress)
                .Include(x => x.OfferDetails).ThenInclude(x => x.SellerContact).Where(e => e.ID > start && e.ID < end);

            if (entries.Count() == 0)
            {
                return new NotFoundResult();
            }

            return Ok(entries);
        }


        [HttpPost]
        public IActionResult Post()
        {
            List<Entry> entries = Integratgion.GetFlatForSalesEntries();
            cleanImagesDirectory();

            if (entries.Count() == 0)
            {
                return new NotFoundResult();
            }
            
            dbContext.Entries.AddRange(entries);
            dbContext.SaveChanges();

            return Ok(entries); ;
        }

        [HttpPost("{page}")]
        public IActionResult Post(int page)
        {
            if (page < 0)
            {
                return new BadRequestResult();
            }

            List<Entry> entries = Integratgion.GetEntryByPage(page);
            cleanImagesDirectory();


            if (entries.Count() == 0)
            {
                return new NotFoundResult();
            }

            dbContext.Entries.AddRange(entries);
            dbContext.SaveChanges();

            return Ok(entries); ;
        }

        [HttpPut("{id}")]
        public IActionResult UpdateEntry(int id, Entry entry)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != entry.ID)
            {
                return BadRequest("Id's did not match");
            }

            dbContext.Update(entry);
            dbContext.SaveChanges();

            return Ok(entry);

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
