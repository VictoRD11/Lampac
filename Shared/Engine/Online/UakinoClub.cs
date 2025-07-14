using Lampac.Engine.CORE;
using Shared.Model.Base;
using Shared.Model.Online.Eneyida;
using Shared.Model.Templates;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;

namespace Shared.Engine.Online
{
    public class UakinoClubInvoke
    {
        #region UakinoClubInvoke
        string? host;
        string apihost;
        Func<string, ValueTask<string?>> onget;
        Func<string, string, ValueTask<string?>> onpost;
        Func<string, string> onstreamfile;
        Func<string, string>? onlog;
        Action? requesterror;

        public UakinoClubInvoke(string? host, string apihost, Func<string, ValueTask<string?>> onget, Func<string, string, ValueTask<string?>> onpost, Func<string, string> onstreamfile, Func<string, string>? onlog = null, Action? requesterror = null)
        {
            this.host = host != null ? $"{host}/" : null;
            this.apihost = apihost;
            this.onget = onget;
            this.onpost = onpost;
            this.onstreamfile = onstreamfile;
            this.onlog = onlog;
            this.requesterror = requesterror;
        }
        #endregion

        #region Embed
        public async ValueTask<EmbedModel?> Embed(string? original_title, int year, string? href)
        {
            if (string.IsNullOrWhiteSpace(href) && (string.IsNullOrWhiteSpace(original_title) || year == 0))
                return null;

            string? link = href;
            var result = new EmbedModel();

            if (string.IsNullOrEmpty(link))
            {
                onlog?.Invoke("search start");
                string? search = await onget.Invoke($"{apihost}/?do=search&subaction=search&story={HttpUtility.UrlEncode(original_title)}");
                if (search == null)
                {
                    requesterror?.Invoke();
                    return null;
                }

                onlog?.Invoke("search ok");

                foreach (string row in search.Split("class=\"movie-item\"").Skip(1))
                {
                    if (string.IsNullOrWhiteSpace(row))
                        continue;

                    string newslink = Regex.Match(row, "href=\"([^\"]+)\"").Groups[1].Value;
                    if (string.IsNullOrWhiteSpace(newslink))
                        continue;

                    string name = Regex.Match(row, "title=\"([^\"]+)\"").Groups[1].Value;
                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    // Check year match
                    string yearStr = Regex.Match(row, @"(\d{4})").Groups[1].Value;
                    if (!string.IsNullOrWhiteSpace(yearStr) && int.TryParse(yearStr, out int movieYear))
                    {
                        if (Math.Abs(movieYear - year) <= 1) // Allow 1 year difference
                        {
                            link = newslink;
                            break;
                        }
                    }

                    if (result.similars == null)
                        result.similars = new List<Similar>();

                    result.similars.Add(new Similar()
                    {
                        title = name,
                        href = newslink
                    });
                }

                if (string.IsNullOrWhiteSpace(link))
                {
                    if (result.similars == null || result.similars.Count == 0)
                        return null;

                    return result;
                }
            }

            onlog?.Invoke("embed start");
            string? html = await onget.Invoke(link);
            if (html == null)
            {
                requesterror?.Invoke();
                return null;
            }

            onlog?.Invoke("embed ok");

            // Extract video sources from the page
            var matches = Regex.Matches(html, @"data-src=[""']([^""']+)[""']");
            if (matches.Count == 0)
            {
                // Try alternative pattern
                matches = Regex.Matches(html, @"src=[""']([^""']+\.m3u8[^""']*)[""']");
            }

            if (matches.Count == 0)
            {
                return null;
            }

            result.content = matches[0].Groups[1].Value;

            return result;
        }
        #endregion

        #region Html
        public string Html(EmbedModel? embed, int clarification, string? title, string? original_title, int year, int t, int s, string? href, VastConf vast = null, bool rjson = false)
        {
            if (embed == null)
                return string.Empty;

            if (embed.similars != null && embed.similars.Count > 0)
            {
                var stpl = new SimilarTpl(embed.similars.Count);
                foreach (var sim in embed.similars)
                {
                    string link = $"{host}lite/uakinoclub?title={HttpUtility.UrlEncode(title)}&original_title={HttpUtility.UrlEncode(original_title)}&year={year}&clarification={clarification}&href={HttpUtility.UrlEncode(sim.href)}";
                    stpl.Append(sim.title, string.Empty, string.Empty, link, string.Empty);
                }

                return rjson ? stpl.ToJson() : stpl.ToHtml();
            }

            if (!string.IsNullOrEmpty(embed.content))
            {
                var mtpl = new MovieTpl(title, original_title, 1);
                string videoLink = $"{host}lite/uakinoclub/video?link={HttpUtility.UrlEncode(embed.content)}";
                mtpl.Append("Оригинал", videoLink, "call", videoLink);
                return rjson ? mtpl.ToJson() : mtpl.ToHtml();
            }

            return string.Empty;
        }
        #endregion
    }
}