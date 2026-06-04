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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CRM.Servise;

namespace CRM.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MessageeServise serviceMessagee;
        WindowService serviceWindow;

        public User CurrentUser { get; }
        public MainWindow()
        {
            InitializeComponent();
           
            Grid_comp.MouseEnter += ImageEnter;
            Grid_comp.MouseLeave += ImageLeave;

            Grid_question.MouseEnter += ImageEnter;
            Grid_question.MouseLeave += ImageLeave;

            Grid_technical.MouseEnter += ImageEnter;
            Grid_technical.MouseLeave += ImageLeave;

            Grid_user.MouseEnter += ImageEnter;
            Grid_user.MouseLeave += ImageLeave;

            Grid_client.MouseEnter += ImageEnter;
            Grid_client.MouseLeave += ImageLeave;

            Grid_exit.MouseEnter += ImageEnter;
            Grid_exit.MouseLeave += ImageLeave;
        }
        private void ImageEnter(object sender, MouseEventArgs e)
        {
            Grid grids = (Grid)sender;
            Label lb = (Label)grids.Children[0];
            Image im = (Image)grids.Children[1];
            Image im2 = (Image)grids.Children[2];


            var animLB = new DoubleAnimation();
            animLB.Duration = TimeSpan.FromMilliseconds(150);
            animLB.From = 0;
            animLB.To = 1;

            lb.BeginAnimation(Image.OpacityProperty, animLB);

            animLB.From = lb.ActualWidth;
            animLB.To = grids.ActualWidth;
            lb.BeginAnimation(WidthProperty, animLB);

            var anim = new DoubleAnimation();

            anim.From = 1;
            anim.To = 0;
            anim.Duration = TimeSpan.FromMilliseconds(150);

            im.BeginAnimation(Image.OpacityProperty, anim);

            anim.From = 0;
            anim.To = 1;
            anim.Duration = TimeSpan.FromMilliseconds(150);
            im2.BeginAnimation(Image.OpacityProperty, anim);

        }
        private void ImageLeave(object sender, MouseEventArgs e)
        {
            Grid grids = (Grid)sender;
            Label lb = (Label)grids.Children[0];
            Image im = (Image)grids.Children[1];
            Image im2 = (Image)grids.Children[2];


            var anim = new DoubleAnimation();
            anim.Duration = TimeSpan.FromMilliseconds(150);
            anim.From = 0;
            anim.To = 1;


            im.BeginAnimation(Image.OpacityProperty, anim);

            anim.From = 1;
            anim.To = 0;
            im2.BeginAnimation(Image.OpacityProperty, anim);


            var animLB = new DoubleAnimation();
            animLB.Duration = TimeSpan.FromMilliseconds(150);
            animLB.From = 1;
            animLB.To = 0;

            lb.BeginAnimation(Image.OpacityProperty, animLB);

            animLB.From = grids.ActualWidth;
            animLB.To = 0;
            lb.BeginAnimation(WidthProperty, animLB);


        }
    }
}
