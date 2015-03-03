﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using uProject.uVersion;
using Marketplace.Interfaces;
using Marketplace.Providers;

namespace uProject.Razor
{
    public class VersionCompatibilityReport
    {
        private List<UVersion> uVersions;
        private int fileId { get; set; }
        private int packageId { get; set; }
        private IListingProvider projectProvider;
        private IListingItem project;
        private IMediaFile file;


        public VersionCompatibilityReport(int fid, int pid)
        {
            uVersions = uProject.uVersion.UVersion.GetAllVersions();
            fileId = fid;
            packageId = packageId;
            projectProvider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];
            project = projectProvider.GetListing(pid, false);
            file = project.PackageFile.FirstOrDefault(x => x.Id == Int32.Parse(project.CurrentReleaseFile));
        }

        public List<verCompat> GetCompatibilityReport()
        {
            var compatList = new List<verCompat>();
            using (Marketplace.Data.MarketplaceDataContext ctx = new Marketplace.Data.MarketplaceDataContext())
            {

                foreach (var ver in uVersions)
                {
                    var reports = ctx.DeliVersionCompatibilities.Where(x => x.version == ver.Name && x.projectId == project.Id);

                    if (reports.Any())
                    {
                        float compats = reports.Count(x => x.isCompatible);
                        float numReps = reports.Count();
                        var perc = Convert.ToInt32(((compats / numReps) * 100));

                        var smiley = "unhappy";

                        if (perc >= 95)
                        {
                            smiley = "joyous";
                        }
                        else if (perc < 95 && perc >= 80)
                        {
                            smiley = "happy";
                        }
                        else if (perc < 80 && perc >= 65)
                        {
                            smiley = "neutral";
                        }
                        else if (perc < 65 && perc >= 50)
                        {
                            smiley = "unhappy";
                        }
                        else
                        {
                            smiley = "superUnhappy";
                        }



                        compatList.Add(new verCompat() { perc = perc, smiley = smiley, version = ver.Name });
                    }
                    else
                    {
                        compatList.Add(new verCompat() { perc = 0, smiley = "untested", version = ver.Name });
                    }

                }
            }
            return compatList;

        }
    }

    public class verCompat
    {
        public int perc { get; set; }
        public string smiley { get; set; }
        public string version { get; set; }
    }
}