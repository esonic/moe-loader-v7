﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SitePack
{
    class SiteProvider
    {
        public List<MoeLoader.ImageSite> SiteList()
        {
            List<MoeLoader.ImageSite> sites = new List<MoeLoader.ImageSite>();

            sites.Add(new SiteLargeBooru(
                //"https://yande.re/post/index.xml?page={0}&limit={1}&tags={2}", //XML
                "http://yande.re/post?page={0}&limit={1}&tags={2}", //HTML
                "https://yande.re/tag/index.xml?limit={0}&order=count&name={1}",
                "yande.re", "yande", "https://yande.re/", false, MoeLoader.BooruProcessor.SourceType.HTML));

            sites.Add(new SiteLargeBooru(
                "http://konachan.com/post/index.xml?page={0}&limit={1}&tags={2}",
                "http://konachan.com/tag/index.xml?limit={0}&order=count&name={1}",
                "konachan.com", "konachan", null, false, MoeLoader.BooruProcessor.SourceType.XML));

            sites.Add(new SiteBooru(
                "http://donmai.us/post?page={0}&limit={1}&tags={2}",
                "http://donmai.us/tag/index.xml?limit={0}&order=count&name={1}",
                "danbooru.donmai.us", "donmai", null, false, MoeLoader.BooruProcessor.SourceType.HTML));

            sites.Add(new SiteBooru(
                "http://behoimi.org/post/index.xml?page={0}&limit={1}&tags={2}",
                "http://behoimi.org/tag/index.xml?limit={0}&order=count&name={1}",
                "behoimi.org", "behoimi", "http://behoimi.org/", false, MoeLoader.BooruProcessor.SourceType.XML));

            //sites.Add(new SiteBooru(
            //    "http://wakku.to/post/index.xml?page={0}&limit={1}&tags={2}",
            //    "http://wakku.to/tag/index.xml?limit={0}&order=count&name={1}",
            //    "wakku.to", "wakku", null, false, BooruProcessor.SourceType.XML));

            //sites.Add(new SiteBooru(
            //    "http://nekobooru.net/post/index.xml?page={0}&limit={1}&tags={2}",
            //    "http://nekobooru.net/tag/index.xml?limit={0}&order=count&name={1}",
            //    "nekobooru.net", "nekobooru", null, false, BooruProcessor.SourceType.XML));

            //sites.Add(new SiteBooru(
            //    "http://idol.sankakucomplex.com/post/index.json?page={0}&limit={1}&tags={2}",
            //    "http://idol.sankakucomplex.com/tag/index.xml?limit={0}&order=count&name={1}",
            //    "idol.sankakucomplex.com", "idol", null, false, MoeLoader.BooruProcessor.SourceType.JSON));

            //sites.Add(new SiteBooru(
            //    "http://chan.sankakucomplex.com/post/index.json?page={0}&limit={1}&tags={2}",
            //    "http://chan.sankakucomplex.com/tag/index.xml?limit={0}&order=count&name={1}",
            //    "chan.sankakucomplex.com", "chan", null, false, MoeLoader.BooruProcessor.SourceType.JSON));
            if (System.IO.File.Exists(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\18x"))
                sites.Add(new SiteSankaku("idol"));

            sites.Add(new SiteSankaku("chan"));

            sites.Add(new SiteBooru(
                "http://safebooru.org/index.php?page=dapi&s=post&q=index&pid={0}&limit={1}&tags={2}",
                "http://safebooru.org/index.php?page=dapi&s=tag&q=index&order=name&limit={0}&name={1}",
                "safebooru.org", "safebooru", null, true, MoeLoader.BooruProcessor.SourceType.XML));

            sites.Add(new SiteBooru(
                "http://gelbooru.com/index.php?page=dapi&s=post&q=index&pid={0}&limit={1}&tags={2}",
                "http://gelbooru.com/index.php?page=dapi&s=tag&q=index&order=name&limit={0}&name={1}",
                "gelbooru.com", "gelbooru", null, true, MoeLoader.BooruProcessor.SourceType.XML));

            //tag
            sites.Add(new SiteEshuu(1));
            //artist
            sites.Add(new SiteEshuu(3));
            //source
            sites.Add(new SiteEshuu(2));
            //chara
            sites.Add(new SiteEshuu(4));

            sites.Add(new SiteZeroChan());

            sites.Add(new SiteMjvArt());

            sites.Add(new SiteWCosplay());

            sites.Add(new SitePixiv(SitePixiv.PixivSrcType.Tag));
            sites.Add(new SitePixiv(SitePixiv.PixivSrcType.Author));
            sites.Add(new SitePixiv(SitePixiv.PixivSrcType.Day));
            sites.Add(new SitePixiv(SitePixiv.PixivSrcType.Week));
            sites.Add(new SitePixiv(SitePixiv.PixivSrcType.Month));

            sites.Add(new SiteMiniTokyo(1));
            sites.Add(new SiteMiniTokyo(2));

            return sites;
        }
    }
}
