﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using Jester.Exceptions;

namespace Jester
{
    public partial class TesterForm : Form
    {
        private Dictionary<string, MethodTestInfo> methodDictionary;
        private const int TEST_STATUS_INDEX = 1;

        public TesterForm()
        {
            InitializeComponent();
            methodDictionary = new Dictionary<string, MethodTestInfo>();
            Assembly asm = Assembly.GetCallingAssembly();
            SetListViewProperties();
            SetupTestsForExecution(asm);
        }

        private void SetupTestsForExecution(Assembly asm)
        {
            var types = asm.GetTypes();
            var allMethods = types.SelectMany(t => t.GetMethods());
            var methodList = allMethods.Where(m => m.GetCustomAttributes(typeof(JestAttribute), false).Length > 0
                && m.IsMethodVoidOrNonValue()).ToArray();
            foreach (var m in methodList)
            {
                    AddTest(m);

            }
        }

        private void AddTest(MethodInfo m)
        {
            ListViewItem.ListViewSubItem[] subItems =
                new ListViewItem.ListViewSubItem[2];

            subItems[0] = new ListViewItem.ListViewSubItem(null, m.Name);
            subItems[1] = new ListViewItem.ListViewSubItem(null, JestResultsEnum.NOT_RUN.display);

            ListViewItem item = new ListViewItem(subItems, 0);
            item.UseItemStyleForSubItems = false;
            listView1.Items.Add(item);
            MethodTestInfo methodTest = new MethodTestInfo()
            {
                MethodName = m.Name,
                Method = m,
                ErrorInfo = String.Empty,
                ListViewSubItem = subItems[TEST_STATUS_INDEX]
            };

            methodDictionary.Add(m.Name, methodTest);
        }

        private void SetListViewProperties()
        {
            listView1.MultiSelect = false;
            listView1.FullRowSelect = false;
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            
            System.Windows.Forms.ListViewItem.ListViewSubItem item = listView1.HitTest(e.Location).SubItem;
            string methodName = item.Text;
            string testResult = SelectTest(methodName);
            textBox1.Text = testResult;
        }

        private string SelectTest(string methodName)
        {
            return SelectTest(methodDictionary[methodName].Method);
        }

        private string SelectTest(MethodInfo method)
        {
            
            MethodTestInfo testInfo = methodDictionary[method.Name];
            return  JesterUtilities.SelectTest(method, testInfo);
        }

        

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            string methodName = listView1.HitTest(e.Location).SubItem.Text;
            textBox1.Text = methodDictionary[methodName].ErrorInfo;
        }

        private void btnRunAll_Click(object sender, EventArgs e)
        {
            foreach (var m in methodDictionary)
            {
                SelectTest(m.Value.Method);   
            }
        }
    }
}
