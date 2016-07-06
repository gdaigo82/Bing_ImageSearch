using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//gdaigo add
using CuiHelperLib;
using System.Net;

namespace BingImageSearch
{
    /*
     *   実処理を行うアプリケーションのサンプルです。
     */

    // 結果をJSONにするアプリです。
    // ファイル名は　検索単語_数字.json です。Resoucresフォルダに入ります。
    // シリアル番号と画像へのURLがリンクされます。
    class MakeJsonFile
    {
        private string MakeFileName(string word, int index)
        {
            return MainWindow.RESOURCES_PATH + word + "_" + index + ".json";
        }

        private string MakeOneData(int serial, string url, bool final)
        {
            string json = "{\"id\":";
            json += serial + ", \"url\":\"" + url;
            if (final == false)
            {
                json += "\"},\r\n";
            }
            else
            {
                json += "\"}\r\n";
            }

            return json;
        }

        public bool Make(string word, int start, int cnt, string[] url)
        {
            string name = MakeFileName(word, start);
            string json = "{\"data\":[\r\n";
            bool final = false;
            for (int i=0; i< cnt; i++)
            {
                if (i == cnt-1)
                {
                    final = true;
                }
                json += MakeOneData(start+i, url[i], final);
            }
            json += "]\r\n}";

            return TextFile.Write(name, false, json);
        }
    }


    // Bing画像検索を行います。
    class PictureSearchByBing
    {
        private const string SETTING_ITEM_NAME = "parameters";
        private const string BING_SUCCESS_IMG = "pronama_execute.png";
        private const string BING_ERROR_IMG = "pronama_error.png";
        private const string BING_SEARCH_API_URL = "https://api.datamarket.azure.com/Bing/Search/";
        private const int BING_SEARCH_MAX = 50;

        private string m_BingKey;
        private string m_exceptionMeggase;
        private string[] m_title;
        private string[] m_mediaUri;
        private int m_numberOfItem;
        private string m_word;
        private string m_latestViewHtmlName;
        private bool m_success;

        private int m_count=0;
        private Bing.BingSearchContainer m_BingContainer;
        private MakeJsonFile m_MakeJson;

        private string GetIndexFileName(int number)
        {
            return "bing" + number.ToString() + ".html";
        }

        private void CallBingSearchAPI(string word)
        {
            // 以下で定義されたメンバ変数を使って検索、結果を格納します。

            // 入力
            // 引数word が検索単語です。
            // 以下のコードで使っている定義です。呼び出す前に設定済です。
            //   private const string BING_SEARCH_API_URL = "https://api.datamarket.azure.com/Bing/Search/";
            //   private const int BING_SEARCH_MAX = 50;
            //   private string m_BingKey = "プライマリ アカウント キー";
            //   private int m_count; // 何回目の検索であるかが設定されています。

            // 出力
            // 以下に成功失敗が入ります。
            //   private bool m_success;
            // 失敗の際には以下にエラー（例外メッセージ）が入ります。
            //   private string m_exceptionMeggase;
            // 成功の際には、以下の個数が入ります。
            //   private int m_numberOfItem;
            // 以下にタイトルと画像のURLをそれぞれ入れます。
            //   private string[] m_title = new string[BING_SEARCH_MAX];
            //   private string[] m_mediaUri = new string[BING_SEARCH_MAX];

            m_numberOfItem = 0;
            try
            {
                m_BingContainer = new Bing.BingSearchContainer(new Uri(BING_SEARCH_API_URL));
                m_BingContainer.Credentials = new NetworkCredential(m_BingKey, m_BingKey);

                var imageQuery = m_BingContainer.Image(word, null, null, null, null, null, null);
                imageQuery = imageQuery.AddQueryOption("$top", BING_SEARCH_MAX);
                imageQuery = imageQuery.AddQueryOption("$skip", m_count *BING_SEARCH_MAX);
                var imageResults = imageQuery.Execute();
                foreach (Bing.ImageResult result in imageResults)
                {
                    m_title[m_numberOfItem] = result.Title;
                    m_mediaUri[m_numberOfItem] = result.MediaUrl;
                    m_numberOfItem++;
                }
                m_success = true;

            }
            catch (Exception e)
            {
                m_exceptionMeggase = e.Message;
                m_success = false;
            }
        }

        private string ExecuteSearch()
        {
            string html = "<h3>「" + m_word + "」の検索結果</h3><p><br>";
            CallBingSearchAPI(m_word);
            if (m_success)
            {
                int start = m_count * BING_SEARCH_MAX + 1;
                int i = 0;
                for (i=0; i<m_numberOfItem; i++)
                {
                    html += (start + i).ToString() + " : " + m_title[i] + "<br>";
                    html += "<a href=\"" + m_mediaUri[i] + "\">";
                    html += m_mediaUri[i];
                    html += "</a><br><br>";
                }
            }
            else
            {
                if (m_exceptionMeggase.Length > 0)
                {
                    html += "「" + m_exceptionMeggase + "」だって。";
                }
                else
                {
                    html += "処理に失敗しました。";
                }
            }
            html += "</P>";
            return html;
        }

        private string GetSelectedHtml(int count)
        {
            if (count > 0)
                count--;
            int number = count / BING_SEARCH_MAX;
            if (number >= m_count)
            {
                return null;
            }
            else
            {
                m_latestViewHtmlName = GetIndexFileName(number);
                return m_latestViewHtmlName;
            }
        }

        // numberで示された数字で指定された画像リンクのあるページを再表示
        public void ViewOtherPage(string number, CuiApplicationResult result)
        {
            result.htmlPath = null;
            try
            {
                int i = int.Parse(number);
                string name = GetSelectedHtml(i);
                if (name != null)
                {
                    result.htmlPath = name;
                    result.imgPath = BING_SUCCESS_IMG;
                    result.serif = number + " 枚目のあるページ出すね";
                }
                else
                {
                    result.imgPath = "pronama_error.png";
                    result.serif = number + " のあるページが見つからないみたい…";
                }
            }
            catch (FormatException e)
            {
                DebugPrint.output("sample", e.Message);
                result.imgPath = BING_ERROR_IMG;
                result.serif = "うーん…" + number + "は数字じゃない？";
            }
        }

        private string GetLatestHtml()
        {
            return m_latestViewHtmlName;
        }

        // BACKの処理です。
        public void Back(CuiApplicationResult result)
        {
            string name = GetLatestHtml();
            result.htmlPath = null;
            result.imgPath = null;
            result.serif = null;
            if (name != null)
            {
                result.htmlPath = name;
                result.imgPath = BING_SUCCESS_IMG;
                result.serif = "検索結果に戻りまーす。";
            }
        }

        private bool MakeJsonFile()
        {
            if (m_success == false)
            {
                return false;
            }
            int start = m_count * BING_SEARCH_MAX + 1;
            return m_MakeJson.Make(m_word, start, m_numberOfItem, m_mediaUri);
        }

        private void MakeResult(string word, string html, bool madeJson, CuiApplicationResult result)
        {
            string message = "";
            if (m_success)
            {
                int start = m_count * BING_SEARCH_MAX + 1;
                int end = m_count * BING_SEARCH_MAX + BING_SEARCH_MAX;
                message += word + " で検索したよ！　結果は上を見てね。\r\n";
                message += start + "~" + end + " 枚目の結果です。\r\n";
                if (madeJson)
                {
                    message += "jsonファイルも作成しました。";
                }
                else
                {
                    message += "jsonファイルの作成には失敗したみたい。";
                }
                result.imgPath = BING_SUCCESS_IMG;
                result.htmlPath = GetIndexFileName(m_count);
                m_latestViewHtmlName = result.htmlPath;
                m_count++;
            }
            else
            {
                message += word + " の検索に失敗したみたい。\r\n";
                if (m_exceptionMeggase.Length > 0)
                {
                    message += "「" + m_exceptionMeggase + "」だって。\r\n";
                }
                result.imgPath = BING_ERROR_IMG;
                result.htmlPath = "bing_err.html";
            }
            result.html = html;
            result.serif = message;

        }

        // NEXT の処理です。
        public void Next(CuiApplicationResult result)
        {
            if (m_count <= 0)
            {
                result.html = null;
                result.imgPath = BING_ERROR_IMG;
                result.serif = "まず検索した方がいいかも";
                return;
            }
            string html = ExecuteSearch();
            bool madeJson = MakeJsonFile();
            MakeResult(m_word, html, madeJson, result);
        }

        // SEARH の処理です。
        public void NewSearch(string word, CuiApplicationResult result)
        {
            if (word.Length <= 0)
            {
                result.html = null;
                result.imgPath = BING_ERROR_IMG;
                result.serif = "入力がおかしいかも。";
                return;
            }
            m_count = 0;
            m_word = word;
            string html = ExecuteSearch();
            bool madeJson = MakeJsonFile();
            MakeResult(m_word, html, madeJson, result);
        }

        // setting.jsonを読み込んで、BinAPIのkeyを取得します。
        public bool MakePrimaryKey()
        {
            CuiHelperJsonParser jSonParser = new CuiHelperJsonParser();
            bool success = jSonParser.StartJson(MainWindow.SETTING_FILE);
            if (!success)
            {
                return false;
            }

            dynamic item = jSonParser.SearchItem(SETTING_ITEM_NAME);
            if (item == null)
            {
                return false;
            }

            m_BingKey = item.PrimayKey;
            DebugPrint.output("Bing", "key=" + m_BingKey);
            return true;
        }

        public PictureSearchByBing()
        {
            m_title = new string[BING_SEARCH_MAX];
            m_mediaUri = new string[BING_SEARCH_MAX];
            m_MakeJson = new MakeJsonFile();
        }
    }
}
