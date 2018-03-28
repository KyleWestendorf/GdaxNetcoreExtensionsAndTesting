using Boukenken.Gdax;
using gdax.netcore.Api.Authentication;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Diagnostics;
using System.Windows.Media.Animation;
using LiveCharts;
using LiveCharts.Wpf;

// to do: Separate the WPF from the logic entirely so that the methods and classes are reusable
// Credit to Gdax Netcore Project on github for source code: https://github.com/sefbkn/gdax.netcore
namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DrawGraphs();
        }

        public void DrawGraphs()
        {
            LineSeries mySeries = new LineSeries
            {
                Values = new ChartValues<int> { 12, 23, 55, 1 }
                
            };

            myChart.Series.Add(mySeries);
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // to do: Add exception handling, recode to simplify with new methods

            Label.Content = Pairs.SelectedValue.ToString();
            string url = "https://api.gdax.com/products/" + Pairs.SelectedValue.ToString() + "/book?level=2";
            HttpWebRequest myHttpWebRequest = (HttpWebRequest) WebRequest.Create(url);
            myHttpWebRequest.UserAgent = ".NET Framework Test Client";
            HttpWebResponse myHttpWebResponse = (HttpWebResponse) myHttpWebRequest.GetResponse();
            Stream streamResponse = myHttpWebResponse.GetResponseStream();
            StreamReader streamRead = new StreamReader(streamResponse);
            Char[] readBuff = new Char[256];
            //int count = streamRead.Read(readBuff, 0, 256);
            string myResult = streamRead.ReadToEnd();
            var coinData = JsonConvert.DeserializeObject<Coin>(myResult);


            for (int i = 0; i < 50; i++)
            {
                string current = coinData.bids[i][0].ToString();
                string bids = string.Format("Bid: {0}", current);
                bidsList.Items.Add(bids);

            }

            for (int i = 0; i < 50; i++)
            {
                string current = coinData.asks[i][0].ToString();
                string asks = string.Format("Asks: {0}", current);
                asksList.Items.Add(asks);
            }

            streamRead.Close();
            streamResponse.Close();
            myHttpWebResponse.Close();
            Console.ReadLine();

           

        }


        // Cancel Orders Button
        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Authenticate authenticate = new Authenticate();
            OrderClient orderClient = authenticate.MakeOrderClient();

            try
            {
                await orderClient.CancelOpenOrdersAsync("ETH-USD");
            }
            catch (Exception exception)
            {
                Label.Content = exception.Message;

            }


        }

        // the below will take your starting bid amount and your total amount, divide in half and place ten bids in increment
        private async void Button_ClickPlaceIncrementedOrder(object sender, RoutedEventArgs e)
        {
            // todo: add exception handling, place methods into client

            Authenticate authenticate = new Authenticate();
            OrderClient orderClient = authenticate.MakeOrderClient();


            decimal holdings = Convert.ToDecimal(StartingAmount.Text);
            decimal holdingsDivided = holdings / 10;
            decimal price = Convert.ToDecimal(StartingPrice.Text);
            string side = BuySell.SelectedValue.ToString();
            string pair = PairsBuySell.SelectedValue.ToString();

            try
            {
                for (decimal i = price; i < 1100; i = i + 10)
                {
                    await orderClient.PlaceOrderAsync(side, pair, holdingsDivided, i, "limit");
                }

                Label.Content = "Successfully placed limit order.";
            }

            catch (Exception ex)
            {
                Label.Content = ex.ToString();
            }


        }

        // Cancel Most Recent Button
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            // todo: add exception handling

            Authenticate authenticate = new Authenticate();
            OrderClient orderClient = authenticate.MakeOrderClient();
            orderClient.CancelMostRecent();

            //Label.Content = message.Message;






        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            //Authenticate authenticate = new Authenticate();
            //OrderClient orderClient = authenticate.MakeOrderClient();
            //ApiResponse<Order> response = await orderClient.GetOpenOrdersAsync();
            //Order order = response.Value;
            //order.price = (order.price * 2);
            //await orderClient.CancelOneOrderAsync(order.id.ToString());
            //await orderClient.PlaceOrderAsync(order.side, order.product_id, order.size, order.price, order.type);

        }

        public async void PostOpenOrders()
        {

            Authenticate authenticate = new Authenticate();
            OrderClient orderClient = authenticate.MakeOrderClient();
            ApiResponse<IEnumerable<Order>> response = await orderClient.GetOpenOrdersAsync();


            foreach (var item in response.Value)
            {
                bidsList.Items.Add(item.id);
            }
           
           
        }



        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            bidsList.Items.Clear();
            asksList.Items.Clear();
        }

        private async void StreamButton_Click(object sender, RoutedEventArgs e)
        {
           // Clicking this will begin text-file logging of response from API for all orders on below pair

            Uri gdaxUri = new Uri("wss://ws-feed.gdax.com");
            string[] products = new[] {"BTC-USD"};
           
            RealtimeClient client = new RealtimeClient(gdaxUri, products);
            Action<string> a = ProcessResult;
            await client.SubscribeAsync(a);
            


        }

         
        public static void ProcessResult(string s)
        {
            // todo: add exception handling, place methods into client

            RealTimeSubscriptionMessage order = JsonConvert.DeserializeObject<RealTimeSubscriptionMessage>(s);

            // Put your required file location into the blank space with your file location to stream the file items

            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(@" ", true))
            {
                
                file.WriteLine(s);

            }
        }
    }
}
