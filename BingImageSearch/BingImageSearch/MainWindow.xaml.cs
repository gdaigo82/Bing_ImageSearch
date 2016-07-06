using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

//add by gdaigo
using CuiHelperLib;

namespace BingImageSearch
{
    /// CuiHelperを使う場合は、このように書きます。
    public partial class MainWindow : Window
    {
        //CuiHelperLibはsetting.jsonファイルを必要とします。
        //そのため、setting.jsonのあるフォルダを通知する必要があります。
        public static string RESOURCES_PATH = ".\\Resources\\";
        public static string SETTING_FILE = RESOURCES_PATH + "setting.json";

        private CuiHelperFactory m_appFactory;
        private CuiApplication m_app;
        private CuiHelperData m_BindData;

        public MainWindow()
        {
            InitializeComponent();
            // CuiHelperAppInterfaceを継承したクラスの生成
            m_app = new CuiApplication();

            // CuiHelperFactoryを生成します。生成時、以下のパラメータを渡してあげて下さい。
            // ・setting.jsonのパス(SETTING_FILEは上で定義してます）
            // ・CuiHelperAppInterfaceを継承したクラスのインスタンス
            // ・Browserクラスのインスタンス
            // ・入力用TextBoxのインスタンス
            // ・入力用ComboBoxのインスタンス
            m_appFactory = new CuiHelperFactory(SETTING_FILE, m_app, m_OutputBrowser, m_InputTextBox, m_InputComboBox);

            // データバインドを行います。
            m_appFactory.SetDataContext(this);

            // バインドデータクラスを取得します。通常はこの記述は不要です。
            // アプリ側でバインドデータにアクセスしないといけない場合のみ
            // 使ってください。
            m_BindData = m_appFactory.GetBindingDataClass();

            // アプリを起動します。
            m_appFactory.Start();
        }

        // ボタン処理など、ComboBoxで選んだアイテムを実行するイベントを設定して下さい。
        // 本アプリケーションでは、Buttonをクリックするとこの関数が呼び出されます。
        private void ClickGo(object sender, RoutedEventArgs e)
        {
            // CuiHelperFactoryにComboBoxで選んだアイテムを実行させます。
            m_appFactory.OnComboBoxEvent();
        }

        // WindowDrop関数が必要です。以下をコピペしちゃってください。
        private void WindowDrop(object sender, DragEventArgs e)
        {
            // CuiHelperFactoryに処理させます。
            m_appFactory.WindowDrop(sender, e);
        }

        // WindowPreviewDragOver関数が必要です。以下をコピペしちゃってください。
        private void WindowPreviewDragOver(object sender, DragEventArgs e)
        {
            // CuiHelperFactoryに処理させます。
            m_appFactory.WindowPreviewDragOver(sender, e);
        }

        // OnKeyDownHandler関数が必要です。以下をコピペしちゃってください。
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            // CuiHelperFactoryに処理させます。
            m_appFactory.OnKeyDownHandler(sender, e);
        }
    }
}
