using EntityFrameWork_03_WPF.Model;
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

namespace EntityFrameWork_03_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static CRCMS_newEntities1 db = new CRCMS_newEntities1();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //1.	Подключиться к таблицам Document, вывести информацию по тем документам по
            //которым завершена работа. 
            //Для того что бы понять завершены ли работы по наряду, необходимо,
            //чтобы в таблице TimerArchive, были записи по данному наряду.
            //(связь между таблицами можно посмотреть в сущностях загруженных таблиц)
            int index = GetTasksCombobox.SelectedIndex;
            switch (index)
            {

                case 0:
                    {
                        #region Case0
                        var Task_01 = db.TimerArchives
             .Where(w => w.DateFinish != null && w.DateStart != null)
             .Select(s => new
             {
                 s.Document.DocumentNumber,
                 s.DateFinish,
                 s.DateStart,
                 s.DurationInSeconds

             }).ToList();

                        CreateGridViewColumns(Task_01.ToArray());
                        DataListView.ItemsSource = Task_01;
                        #endregion
                    }
                    break;
                case 1:
                    {
                        #region Case1
                        //2.	Имея список завершенных нарядов 
                        //(завершенные наряды хранятся в таблице TimerArchive),
                        //необходимо посчитать количество полезного времени, 
                        //проведенного пользователями в работе и бесполезного.

                        //Полезным временем считаться данные которые храниться в таблице Timer,
                        // а бесполезное   время считаются данные которые лежат в таблице TimerInactivity.
                        // Время, затраченное по каждой записи в таблице TimerArchive можно рассчитать,
                        // как DateFinish – DateStart.
                        DateTime? start = DatePickerStart.SelectedDate;
                        DateTime? end = DatePickerEnd.SelectedDate;

                        List<TimerArchive> timeArchives = db.TimerArchives.ToList();

                        if (start != null && start != DateTime.MinValue)
                            timeArchives = timeArchives.Where(w => w.DateFinish >= start).ToList();

                        if (end != null && end != DateTime.MinValue)
                            timeArchives = timeArchives.Where(w => w.DateFinish <= end).ToList();

                        var finishDoc = timeArchives
                                        .Select(s => s.DocumentId)
                                        .Distinct();

                        var notuseful = db.TimerInactivityArchives
                                        .Where(w => finishDoc.Contains(w.DocumentId))
                                        .ToList();

                        var zav = db.Documents
                            .Where(w => finishDoc.Contains(w.DocumentId))
                            .Select(s => new
                            {
                                //UseFulTime = timeArchives
                                //            .Where(w1 => w1.DocumentId == s.DocumentId)
                                //            .Sum(su => su.DurationInSeconds),

                                //NotUseFull = notuseful
                                //            .Where(q => q.DocumentId == s.DocumentId)
                                //            .Sum(su => su.DurationInSeconds),

                                s.DocumentNumber,
                                s.ModelId,
                                s.DocumentCreateDate,
                                s.SmcsCode
                            }).ToList();

                        CreateGridViewColumns(zav.ToArray());
                        DataListView.ItemsSource = zav;
                        #endregion
                    }
                    break;
                case 2:
                    {
                        #region Case3
                        //6.	Подсчитать количество времени, проведенного в работе каждого пользователя, 
                        //который участвовал в работе над нарядом. (данные как с таблицы Timer так и с таблицы TimerArchive)

                        List<int> users = db.Timers
                            .Where(w=>w.UserId != 0)
                            .Select(s => (int)s.UserId)
                            .Distinct()
                            .ToList();

                        List<Timer> timers = new List<Timer>();

                        foreach(var it in users)
                        {
                            var sum = db.Timers
                                .Where(w => w.UserId == it)
                                .Sum(s => s.DurationInSeconds);

                            Timer t = new Timer();
                            t.UserId = it;
                            t.DurationInSeconds = sum;
                            timers.Add(t);
                        }

                        var result = timers.Select(s => new
                        {
                            s.UserId,
                            s.DurationInSeconds
                        }).ToList();

                        CreateGridViewColumns(result.ToArray());
                        DataListView.ItemsSource = result;


                        #endregion
                    }
                    break;
                default:
                    break;
            }







        }
        private void CreateGridViewColumns(object[] arr)
        {
            DataGridView.Columns.Clear();
            foreach (var item in arr[0].GetType().GetProperties())
            {
                GridViewColumn a = new GridViewColumn()
                {
                    Header = item.Name,
                    DisplayMemberBinding = new Binding(item.Name)
                };
                DataGridView.Columns.Add(a);
            }

        }
    }
}
