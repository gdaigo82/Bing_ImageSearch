using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//gdaigo add
using CuiHelperLib;

namespace BingImageSearch
{
    /*
     *   ここで作成したアプリケーションとGUIとの関係を記述します。
     *   ここを書き換えることで、任意の処理を行うイメージです。
     *   但し、実際には別のクラスで処理を行い、本コードを利用する方が
     *   楽ではないかと思います。
     */

    // 後、exeと同じフォルダにあるsetting.jsonの設定もお願いします。

    // 本サンプルでは実処理は別クラスで生成し、
    // 各アプリがこのクラスのメンバ変数を設定する作りにしています。

    // これは応答内容を一括でまとめたものです。Sample側のアプリにこれを
    // 設定してもらうような作りになっています。
    public class CuiApplicationResult
    {
        public string html;     // 結果をHTMLにしたもの。
        public string htmlPath; // HTMLファイルに残すのでそのファイル名
        public string serif;    // 右下のTextBoxに表示するもの 
        public string imgPath;  // 左下に出力するイメージファイル
    }

    class CuiApplication :  CuiHelperAppInterface
    {
        private const string BING_SEARCH_API_URL = "https://datamarket.azure.com/dataset/explore/bing/search";

        private const int DELAY_MSEC_FOR_EVENT = 1000;
        private const string NORMAL_IMG = "pronama_normal.png";
        private const string ERROR_IMG = "pronama_error.png";

        private const string COMMAND_GO_HOME = "GO_HOME";
        private const string COMMAND_SEARCH = "SEARCH";
        private const string COMMAND_NEXT = "NEXT";
        private const string COMMAND_BACK = "BACK";

        private CuiHelperComboBoxData[] m_ComboBoxData;
        private CuiHelperBot m_bot;
        private CuiHelperBrowser m_browser;
        private string m_contentsPath;

        private PictureSearchByBing m_searchApp;

        //ここでComboBoxで表示するデータの定義を行います。
        private void MakeComboBoxData()
        {
            //CuiHelperComboBoxData[]の生成
            m_ComboBoxData = new[] {
                new CuiHelperComboBoxData { Name = "Goto Home", Commnad = COMMAND_GO_HOME },
                new CuiHelperComboBoxData { Name = "Search", Commnad = COMMAND_SEARCH },
                new CuiHelperComboBoxData { Name = "Next", Commnad = COMMAND_NEXT },
                new CuiHelperComboBoxData { Name = "Back", Commnad = COMMAND_BACK },
            };
        }

        // 初期イメージを表示します。
        private void MakeInitView(string serif)
        {
            string url = BING_SEARCH_API_URL;
            m_browser.SetURL(@url);
            m_bot.Play(NORMAL_IMG, serif);
        }

        // アプリから戻されたCuiApplicationResultを元に出力します。
        private void MakeReport(CuiApplicationResult result)
        {
            bool htmlValid = false;
            if (result.htmlPath != null)
            {
                string url = m_contentsPath + result.htmlPath;
                if (result.html != null)
                {
                    htmlValid = m_browser.MakeHtmlWithTemplate(url, result.html);
                }
                else
                {
                    // 指定されたファイルがあることを信頼したコードです。
                    htmlValid = true;
                }
                if (htmlValid == true)
                {
                    m_browser.SetURL(url);
                }
            }
            if (result.imgPath != null && result.serif != null)
            {
                m_bot.Play(result.imgPath, result.serif);
            }
        }

        // ドラッグ＆ドロップの前処理。
        public int PrepareDragAndDrop(string[] files, string text)
        {
            return 0;
        }

        // ドラッグ＆ドロップの本処理
        public void DragAndDrop(string[] files, string text)
        {
        }

        // テキスト内容を変更されたというイベントの前処理。
        public int PrepareTextBoxEvent(string text)
        {
            m_bot.Play("pronama_textbox.png", text + " 枚目って事かー");
            return DELAY_MSEC_FOR_EVENT;
        }

        // テキスト内容を変更されたというイベントの本処理。
        public void TextBoxEvent(string text)
        {
            CuiApplicationResult result = new CuiApplicationResult();
            m_searchApp.ViewOtherPage(text, result);
            if (result.htmlPath != null)
            {
                m_browser.SetURL(m_contentsPath + result.htmlPath);
            }
            m_bot.Play(result.imgPath, result.serif);
        }

        // ComboBoxの選択項目実行を行うイベントの前処理。
        public int PrepareComboBoxEvent(string command, string text)
        {
            string serif = "ボタンを押しましたねー！";
            if (command.Equals(COMMAND_BACK))
            {
                // 表示は直ちに。
                return 0;
            }

            // 検索中はそれっぽいメッセージに。
            if (command.Equals(COMMAND_SEARCH) || command.Equals(COMMAND_NEXT))
            {
                serif = "検索するねー。";
            }

            m_bot.Play("pronama_listbox.png", serif);
            return DELAY_MSEC_FOR_EVENT;
        }

        // ComboBoxの選択項目実行を行うイベントの本処理。
        public void ComboBoxEvent(string command, string text)
        {
            CuiApplicationResult result = new CuiApplicationResult();

            if (command.Equals(COMMAND_GO_HOME))
            {
                // 初期画面に戻します。
                MakeInitView("最初の画面に戻したよ。");
            }
            else if (command.Equals(COMMAND_BACK))
            {
                m_searchApp.Back(result);
                MakeReport(result);
            }
            else if (command.Equals(COMMAND_SEARCH))
            {
                m_searchApp.NewSearch(text, result);
                MakeReport(result);
            }
            else if (command.Equals(COMMAND_NEXT))
            {
                m_searchApp.Next(result);
                MakeReport(result);
            }
            else
            {
                m_bot.Play("pronama_execute.png", command + "," + text + "は何だろう？");
            }
        }

        // 生成したCuiHelperComboBoxDataを渡します。
        public CuiHelperComboBoxData[] GetComboBoxData()
        {
            return m_ComboBoxData;
        }

        // リソースの初期化です。
        public void Init(CuiHelperBot bot, CuiHelperBrowser browser, string contentsPath)
        {
            m_bot = bot;
            m_browser = browser;
            m_contentsPath = contentsPath;
            MakeComboBoxData();
            m_searchApp = new PictureSearchByBing();

            if (m_searchApp.MakePrimaryKey() == true)
            {
                MakeInitView("始めましょー！");
            }
            else
            {
                m_bot.Play(ERROR_IMG, "setting.jsonが正しくないかも。");
            }
        }
    }
}
