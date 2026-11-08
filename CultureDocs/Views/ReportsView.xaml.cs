using System;
using System.Collections.Generic;
using System.Windows;
using CultureDocs.Services;

namespace CultureDocs.Views
{
    public partial class ReportsView : Window
    {
        private List<AnalyticsRow> _data;

        public ReportsView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadReport();
        }

        private void LoadReport()
        {
            try
            {
                _data = AnalyticsService.GetAnalytics();
                GridReport.ItemsSource = _data;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            LoadReport();
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (_data == null || _data.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта.");
                return;
            }

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.Filter = "CSV (*.csv)|*.csv";
            dlg.FileName = "Отчёт_" + DateTime.Now.ToString("yyyyMMdd") + ".csv";

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    ExcelExportService.ExportAnalyticsToCsv(_data, dlg.FileName);
                    MessageBox.Show("Сохранено: " + dlg.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}