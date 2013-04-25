﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MoeLoader
{
    /// <summary>
    /// Interaction logic for ImgControl.xaml
    /// 缩略图面板中的图片用户控件
    /// </summary>
    public partial class ImgControl : UserControl
    {
        private Img img;
        public Img Image
        {
            get { return img; }
        }
        private string needReferer;

        private int index;
        private bool canRetry = false;
        private bool isRetrievingDetail = false, isDetailSucc = false;
        private bool imgLoaded = false;
        private bool isChecked = false;

        private System.Net.HttpWebRequest req;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="img">图片</param>
        /// <param name="index">位置索引</param>
        /// <param name="isRed">是否已经浏览过</param>
        /// <param name="needReferer">是否需要伪造Referer</param>
        public ImgControl(Img img, int index, string referer, bool supportScore)
        {
            this.needReferer = referer;
            this.InitializeComponent();
            this.img = img;
            this.index = index;

            if (img.IsViewed)
                //statusBorder.Background = new SolidColorBrush(Color.FromArgb(0xCC, 0xFE, 0xE2, 0xE2));
                statusBorder.Background = new SolidColorBrush(Color.FromArgb(0xEE, 0xE9, 0x93, 0xAA));

            //try
            //{
                //string s = .Substring(img.Score.IndexOf(' '), img.Score.Length - img.Score.IndexOf(' '));
            //score.Text = img.Score.ToString();
            //}
            //catch { }

            if (!supportScore)
            {
                //brdScr.Visibility = System.Windows.Visibility.Hidden;
            }
            
            //chk.Text = img.Dimension;

            //RenderOptions.SetBitmapScalingMode(preview, BitmapScalingMode.Fant);
            preview.DataContext = img;

            preview.SizeChanged += new SizeChangedEventHandler(preview_SizeChanged);
            preview.ImageFailed += new EventHandler<ExceptionRoutedEventArgs>(preview_ImageFailed);
            preview.MouseUp += new MouseButtonEventHandler(preview_MouseUp);
            statusBorder.MouseUp += new MouseButtonEventHandler(preview_MouseUp);
            chk.MouseUp += new MouseButtonEventHandler(preview_MouseUp);
            txtDesc.MouseUp += new MouseButtonEventHandler(preview_MouseUp);

            downBtn.MouseUp += new MouseButtonEventHandler(Border_MouseUp);
            magBtn.MouseUp += new MouseButtonEventHandler(preview_Click);

            //chk.Click += chk_Checked;

            //ToolTip tip = preview.ToolTip as ToolTip;
            //tip.PlacementTarget = preview.Parent as UIElement;
            //TextBlock desc = (tip.Content as Border).Child as TextBlock;

            //下载缩略图
            //DownloadImg();

            if (img.DownloadDetail != null)
            {
                //need detail
                LayoutRoot.IsEnabled = false;
                //isRetrievingDetail = true;
                chk.Text = "信息尚未加载";
            }
            else
            {
                ShowImgDetail();
            }
        }

        void ShowImgDetail()
        {
            chk.Text = img.Dimension;
            string type = "N/A";
            if (img.OriginalUrl.Length > 6)
            {
                type = img.OriginalUrl.Substring(img.OriginalUrl.Length - 3, 3).ToUpper();
            }
            else
            {
                //不应该有这么短的url，有问题
                LayoutRoot.IsEnabled = false;
                chk.Text = "原始地址无效";
                return;
            }
            score.Text = img.Score.ToString();
            txtDesc.Inlines.Add(img.Id + " " + img.Desc);
            txtDesc.Inlines.Add(new LineBreak());
            txtDesc.Inlines.Add(type);
            //txtDesc.Inlines.Add(new LineBreak());
            txtDesc.Inlines.Add(" " + img.FileSize);
            txtDesc.ToolTip = img.Id + " " + img.Desc + "\r\n" + type + "  " + img.FileSize + "  " + img.Date;
            //txtDesc.Inlines.Add(new LineBreak());
            //txtDesc.Inlines.Add("评分: " + img.Score);
            //txtDesc.Inlines.Add(new LineBreak());
            //txtDesc.Inlines.Add("时间: " + img.Date);
            isDetailSucc = true;
        }

        /// <summary>
        /// 下载图片
        /// </summary>
        public void DownloadImg()
        {
            if (PreFetcher.Fetcher.PreFetchedImg(img.PreviewUrl) != null)
            {
                preview.Source = PreFetcher.Fetcher.PreFetchedImg(img.PreviewUrl);
                //preview.Source = BitmapDecoder.Create(PreFetcher.Fetcher.PreFetchedImg(img.PreUrl), BitmapCreateOptions.None, BitmapCacheOption.OnLoad).Frames[0];
            }
            else
            {
                try
                {
                    req = System.Net.WebRequest.Create(img.PreviewUrl) as System.Net.HttpWebRequest;
                    req.Proxy = MainWindow.WebProxy;

                    req.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";
                    if (needReferer != null)
                        //req.Referer = img.PreUrl.Substring(0, img.PreUrl.IndexOf('/', 7) + 1);
                        req.Referer = needReferer;

                    //异步下载开始
                    req.BeginGetResponse(new AsyncCallback(RespCallback), req);
                }
                catch (Exception ex)
                {
                    Program.Log(ex, "Start download preview failed");
                    StopLoadImg();
                }
            }

            if (!isDetailSucc && img.DownloadDetail != null)
            {
                isRetrievingDetail = true;
                chk.Text = "信息加载中...";
                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback((o) =>
                {
                    try
                    {
                        img.DownloadDetail(img, MainWindow.WebProxy);
                        Dispatcher.Invoke(new VoidDel(() =>
                        {
                            LayoutRoot.IsEnabled = true;

                            ShowImgDetail();

                            isRetrievingDetail = false;
                            if (imgLoaded && ImgLoaded != null)
                                ImgLoaded(index, null);
                        }));
                    }
                    catch (Exception ex)
                    {
                        Program.Log(ex, "Download img detail failed");
                        Dispatcher.Invoke(new VoidDel(() =>
                        {
                            isRetrievingDetail = false;
                            canRetry = true;
                            chk.Text = "信息加载失败";
                            if (imgLoaded && ImgLoaded != null)
                                ImgLoaded(index, null);
                        }));
                    }
                }));
            }
        }

        /// <summary>
        /// 异步下载结束
        /// </summary>
        /// <param name="req"></param>
        private void RespCallback(IAsyncResult req)
        {
            try
            {
                System.Net.WebResponse res = ((System.Net.HttpWebRequest)(req.AsyncState)).EndGetResponse(req);
                System.IO.Stream str = res.GetResponseStream();

                Dispatcher.BeginInvoke(new VoidDel(delegate()
                {
                    //BitmapFrame bmpFrame = BitmapDecoder.Create(str, BitmapCreateOptions.None, BitmapCacheOption.OnLoad).Frames[0];

                    //bmpFrame.DownloadCompleted += new EventHandler(bmpFrame_DownloadCompleted);
                    //preview.Source = bmpFrame;
                    preview.Source = BitmapDecoder.Create(str, BitmapCreateOptions.None, BitmapCacheOption.OnLoad).Frames[0];
                }));
            }
            catch (Exception ex)
            {
                Program.Log(ex, "Download preview failed");
                Dispatcher.Invoke(new UIdelegate(delegate(object sender) { StopLoadImg(); }), "");
            }
        }

        //void bmpFrame_DownloadCompleted(object sender, EventArgs e)
        //{
        //    System.Windows.Media.Animation.Storyboard sb = FindResource("imgLoaded") as System.Windows.Media.Animation.Storyboard;
        //    //sb.Completed += ClarifyImage;
        //    sb.Begin();

        //    lt.Visibility = System.Windows.Visibility.Collapsed;

        //    if (ImgLoaded != null)
        //        ImgLoaded(index, null);
        //}

        void preview_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.LeftButton == MouseButtonState.Released)
            {
                if (selBorder.Opacity == 0)
                {
                    chk_Checked(true);
                }
                else
                {
                    chk_Checked(false);
                }
            }
        }

        void preview_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.LeftButton == MouseButtonState.Released)
            {
                if (imgClicked != null)
                    imgClicked(index, null);
            }
        }

        private void chk_Checked(bool isChecked)
        {
            //未改变
            if (this.isChecked == isChecked) return;

            if (isChecked)
            {
                selBorder.Opacity = 1;
                selRec.Opacity = 1;
            }
            else
            {
                selBorder.Opacity = 0;
                selRec.Opacity = 0;
            }

            this.isChecked = isChecked;
            if (checkedChanged != null)
                checkedChanged(index, null);
        }

        /// <summary>
        /// 停止缩略图加载
        /// </summary>
        public void StopLoadImg()
        {
            if (req != null)
                req.Abort();
            preview.Source = new BitmapImage(new Uri("/Images/pic.png", UriKind.Relative));

            //itmRetry.IsEnabled = true;
            canRetry = true;
            //if (ImgLoaded != null)
            //ImgLoaded(this, null);
        }

        /// <summary>
        /// 设置是否选择复选框
        /// </summary>
        /// <param name="isChecked"></param>
        public bool SetChecked(bool isChecked)
        {
            if (!isDetailSucc) return false;
            //chk.IsChecked = isChecked;
            chk_Checked(isChecked);
            return true;
        }

        void preview_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            StopLoadImg();

            //if (ImgLoaded != null)
            //ImgLoaded(this, null);
        }

        /// <summary>
        /// 图像加载完毕
        /// </summary>
        public event EventHandler ImgLoaded;
        public event EventHandler checkedChanged;
        public event EventHandler imgClicked;
        public event EventHandler imgDLed;

        void preview_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 1 && e.NewSize.Height > 1)
            {
                //if (GlassHelper.GetForegroundWindow() == MainWindow.Hwnd)
                //{
                //窗口有焦点才进行动画
                preview.Stretch = Stretch.Uniform;
                System.Windows.Media.Animation.Storyboard sb = FindResource("imgLoaded") as System.Windows.Media.Animation.Storyboard;
                //sb.Completed += new EventHandler(delegate { preview.Stretch = Stretch.Uniform; });
                sb.Begin();

                lt.Visibility = System.Windows.Visibility.Collapsed;
                //}
                //else
                //{
                //    preview.Stretch = Stretch.Uniform;
                //    preview.Opacity = 1;
                //    ((preview.RenderTransform as TransformGroup).Children[0] as ScaleTransform).ScaleX = 1;
                //    ((preview.RenderTransform as TransformGroup).Children[0] as ScaleTransform).ScaleY = 1;
                //}

                imgLoaded = true;
                if (!isRetrievingDetail && ImgLoaded != null)
                    ImgLoaded(index, null);
            }
        }

        /// <summary>
        /// 加入下载队列
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Border_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.LeftButton == MouseButtonState.Released)
            {
                if (imgDLed != null)
                    imgDLed(index, null);
            }
        }

        //private void preB_MouseEnter(object sender, MouseEventArgs e)
        //{
            //RenderOptions.SetBitmapScalingMode(preview, BitmapScalingMode.Fant);
            //preB.Width = preview.ActualWidth + 20;
            //preB.Height = preview.ActualHeight + 20;
            //System.Windows.Media.Animation.Storyboard sb = FindResource("OnMouseEnter1") as System.Windows.Media.Animation.Storyboard;
            //sb.Begin();
        //}

        //private void preB_MouseLeave(object sender, MouseEventArgs e)
        //{
            //RenderOptions.SetBitmapScalingMode(preview, BitmapScalingMode.NearestNeighbor);
            //preB.Width = preview.ActualWidth + 20;
            //preB.Height = preview.ActualHeight + 20;
            //System.Windows.Media.Animation.Storyboard sb = FindResource("OnMouseLeave1") as System.Windows.Media.Animation.Storyboard;
            //sb.Begin();
        //}

        //private void Storyboard_Completed(object sender, EventArgs e)
        //{
        //    RenderOptions.SetBitmapScalingMode(preview, BitmapScalingMode.NearestNeighbor);
        //}

        //private void MenuItem_Click(object sender, RoutedEventArgs e)
        //{
        //    RetryLoad();
        //}

        public void RetryLoad()
        {
            if (canRetry)
            {
                //itmRetry.IsEnabled = false;
                canRetry = false;
                preview.Opacity = 0;
                preview.Stretch = Stretch.None;
                lt.Visibility = System.Windows.Visibility.Visible;
                preview.Source = null;
                ScaleTransform trans = (ScaleTransform)(((TransformGroup)(preview.RenderTransform)).Children[0]);
                trans.ScaleX = 0.6;
                trans.ScaleY = 0.6;

                System.Windows.Media.Animation.Storyboard sb = FindResource("imgLoaded") as System.Windows.Media.Animation.Storyboard;
                sb.Stop();

                DownloadImg();
            }
        }

        private void txtDesc_Click_1(object sender, RoutedEventArgs e)
        {
            //ori
            try
            {
                Clipboard.SetText(img.OriginalUrl);
            }
            catch (Exception) { }
        }

        private void txtDesc_Click_2(object sender, RoutedEventArgs e)
        {
            //sample
            try
            {
                Clipboard.SetText(img.SampleUrl);
            }
            catch (Exception) { }
        }

        private void txtDesc_Click_3(object sender, RoutedEventArgs e)
        {
            //preview
            try
            {
                Clipboard.SetText(img.PreviewUrl);
            }
            catch (Exception) { }
        }

        private void txtDesc_Click_4(object sender, RoutedEventArgs e)
        {
            //tag
            try
            {
                Clipboard.SetText(img.Desc);
            }
            catch (Exception) { }
        }
    }
}