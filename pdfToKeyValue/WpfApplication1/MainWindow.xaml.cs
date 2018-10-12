using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Spire.Pdf;
using System.IO;
using System.Drawing;
using Spire.Pdf.General.Find;
using Spire.Pdf.Exporting;
using HtmlAgilityPack;
using LitJson;
using System.Text.RegularExpressions;


namespace WpfApplication1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    /// 
    
    public class MyTable
    {
        public const int TYPE_UNABLE = -1;//不适用的
        public const int TYPE_CONTENT = 0;//内容
        public const int TYPE_TITLE_UP = 1;//标题 对应内容在上边
        public const int TYPE_TITLE_DOWN = 2;//标题 对应内容在下边
        public const int TYPE_TITLE_LEFT = 3;//标题 对应内容在左边
        public const int TYPE_TITLE_RIGHT = 4;//标题 对应内容在右边
        public const int TYPE_MIX = 11;//标题和内容混合的
        public const int TYPE_JOB_SET_UP = 12;//标题-工作准备(比较特殊单独列出)
        public const int TYPE_TITLE_SMALL = 13;//小标题-工作准备中的小标题(比较特殊单独列出)


        public float X;
        public float Y;
        public float Width;
        public float Height;
        public int   Type = TYPE_CONTENT;//当前表格类型及对应内容位置 0:内容  1\2\3\4:标题 1:对应内容在Up 2:对应内容在Down 3:对应内容在Left 4:对应内容在Right
        public string Text = "";
        public string Image = "";
        public int   row;
        public int   col;
        public int   page;

        //用来Find的list
        public static List<MyTable> [] m_listTables;

        //找一横排table
        public List<MyTable> FindRow()
        {
            List<MyTable> listTable = new List<MyTable>();
            foreach (MyTable table in m_listTables[this.page])
            {
                if (table.row == this.row )
                {
                    listTable.Add(table);
                }
            }

            return listTable;
        }

        //找本页某个table
        public MyTable Find(int row, int col)
        {
            foreach (MyTable table in m_listTables[this.page])
            {
                if (table.row == row && table.col == col)
                {
                    return table;
                }
            }

            return null;
        }

        //找某页某个table
        public MyTable Find(int page, int row, int col)
        {
            foreach (MyTable table in m_listTables[page])
            {
                if (table.row == row && table.col == col)
                {
                    return table;
                }
            }

            return null;
        }

        public MyTable Up()
        {
            return Find(this.row - 1, this.col);
        }

        public MyTable Down()
        {
            MyTable table = Find(this.row + 1, this.col);
            if (table == null)
                table = Find(this.page + 1, 3, this.col);

            return table;
        }

        public MyTable Left()
        {
            return Find(this.row, this.col - 1);
        }

        public MyTable Right()
        {
            return Find(this.row, this.col + 1);
        }
    }

    public class MyContent
    {
        public float X;
        public float Y;
        public string content;
        public bool IsUsed = false;
    }

    public class MyDictionary
    {
        public string Key;
        public string Value;

        public MyDictionary(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }
    }

    /*
    //工具设备
    public class TOOL_AND_EQUIPMENT_
    {
        public string NUM;
        public string PN;
        public string DESCRIPTION;
        public string NEC;
        public string QT;
        public string NOTE;
    }

    //消耗器材
    public class Expendable_
    {
        public string NUM;
        public string PN;
        public string DESCRIPTION;
        public string NEC;
        public string QT_STATUS;
        public string QT;
        public string NOTE;
    }

    //零部件
    public class Part_
    {
        public string NUM;
        public string PN;
        public string DESCRIPTION;
        public string NEC;
        public string QT_STATUS;
        public string QT;
        public string NOTE;
    }

    public class TOOLS_AND_EQUIPMENTS_
    {
        public List<TOOL_AND_EQUIPMENT_> TOOLS_AND_EQUIPMENTS;
        public string NOTE_FOR_SELECTION;
    }

    public class Expendables_
    {
        public List<Expendable_> Expendables;
        public string NOTE_FOR_SELECTION;
    }

    public class Parts_
    {
        public List<Part_> Parts;
        public string NOTE_FOR_SELECTION;
    }

    public class TOOLS_AND_SPARES_
    {
        public TOOLS_AND_EQUIPMENTS_ TOOLS_AND_EQUIPMENTS;
        public Expendables_ Expendables;
        public Parts_ Parts;
    }

    public class JOB_CARD
    {
        public string JOB_CARD_NO;
        public string REV;
        public string AC_TYPE;
        public string OF;
        public string Registration;
        public string Origin;
        public string Reference_Date_Revision;
        public string Interval_Requirement;
        public string Applicability;
        public string Card_Type;
        public string SKILL;
        public string Workarea;
        public string Zone;
        public string Access;
        public string MP_MTOP_Item;
        public string Task_Type;
        public string Inspect_level;
        public string Man_hour;
        public string Personnel;
        public string Elapse_time;
        public string Related_card;
        public string TITLE;
        public string Editor_Date;
        public string Checked_By_Date;
        public string Approved_By_Date;
        public string Actual_MH;
        public string Accomplished_By;
        public string Station_Date;
        public string Note;
        public string Revise_Note;
        public string Special_Requirement;
        public string JOB_SET_UP;
        public string PROCEDURE;
        public string PERF_BY;
        public string INSP_BY;
        public List<string> FIGURE;
        public string JOB_CARD_FEEDBACK_SHEET;
    }
    */

    public partial class MainWindow : Window
    {
        public const float DISTANCE_TOO_SHORT = 2.5f;

        public float[] m_fPageHeights;

        public MainWindow()
        {
            InitializeComponent();
        }
         
        //pdf转SVG 返回总页数
        private int PdfToSvg(string pdfPath, string svgPath)
        {
            //打开pdf
            PdfDocument pdfDoc = new PdfDocument();
            pdfDoc.LoadFromFile(pdfPath);
            int nPageCount = pdfDoc.Pages.Count;

            //获取每页页高
            m_fPageHeights = new float[nPageCount];
            for (int i = 0; i < nPageCount; i++)
            {
                m_fPageHeights[i] = pdfDoc.Pages[i].Size.Height;
            }
            //保存为SVG格式文件
            pdfDoc.SaveToFile(svgPath, FileFormat.SVG);
            pdfDoc.Close();
            return nPageCount;
        }

        //检查两个点是否在水平或垂直线上
        private bool IsOneLine(float f1, float f2)
        {
            if (Math.Abs(f1 - f2) < DISTANCE_TOO_SHORT)
                return true;

            return false;
        }

        //检查两个点是否是同一个点
        private bool IsOnePoint(float fX1, float fY1, float fX2, float fY2)
        {
            if (IsOneLine(fX1, fX2) && IsOneLine(fY1, fY2))
                return true;

            return false;
        }

        //检查内容是否在表格里
        private bool IsInTable(MyTable table, MyContent content)
        {
            if (content.X > table.X && content.Y > table.Y && content.X < table.X + table.Width && content.Y < table.Y + table.Height)
                return true;
            return false;
        }

        //检查是否大表格套小表格
        private bool IsInTable(MyTable table, MyTable tableInside)
        {
            if (tableInside.X > table.X && tableInside.Y > table.Y && tableInside.X < table.X + table.Width && tableInside.Y < table.Y + table.Height)
                return true;
            return false;
        }

        //检查是否线长过短
        private bool IsTooShort(float f1, float f2)
        {
            return IsOneLine(f1,f2);
        }

        //检查是否已经在表里存在
        private bool IsAlreadyInTable(List<MyTable> list, float x, float y, float distance)
        {
            bool ret = false;
            for (int i = 0; i < list.Count; i++)
            {
                if (IsOnePoint(list[i].X, list[i].Y, x, y))
                {
                    if (list[i].Width + list[i].Height >= distance)
                    {
                        ret = true;
                        break;
                    }
                    else
                    {
                        list.Remove(list[i]);
                    }
                }
            }

            return ret;
        }
        
        //检查是否是数字
        public bool IsNumber(string value)
        {
            return Regex.IsMatch(value, @"^[+-]?\d*[.]?\d*$");
        }

        Dictionary<string, int> m_dictUsedCount = new Dictionary<string, int>();
        //获取使用此字符串的次数
        public int GetUsedCount(string key)
        {
            if (m_dictUsedCount.ContainsKey(key))
            {
                m_dictUsedCount[key] += 1;
            }
            else
            {
                m_dictUsedCount.Add(key,1);
            }

            return m_dictUsedCount[key];
        }
        //根据表格文字获取表格类型
        private int GetTableType(string str)
        {
            str = str.Replace(" ", "");

            if (str == "工作卡号JOBCARDNO." && GetUsedCount(str) == 1)
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "版次REV" && GetUsedCount(str) == 1)
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "机型A/CTYPE" && GetUsedCount(str) == 1)
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "页号PAGE")
                return MyTable.TYPE_UNABLE;
            else if (str == "总页OF" && GetUsedCount(str) == 1)
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "Registration机号")
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "Origin来源")
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "Reference/Date/Revision参考文件/日期/版次")
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "IntervalRequirement间隔要求")
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "Applicability适用性")
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "CardType工卡类别")
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "SKILL专业")
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "Workarea工种分区")
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "Zone区域")
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "Access接近")
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "MP/MTOPItemMP/MTOP项目号")
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "TaskType工作类别")
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "Inspectlevel检验级别")
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "Manhour参考工时")
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "Personnel人数")
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "Elapsetime停场时间")
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "Relatedcard参考工卡")
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "TITLE标题")
                return MyTable.TYPE_TITLE_RIGHT;
            else if (str == "Editor/Date编写者/日期")
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "CheckedBy/Date校对者/日期")
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "ApprovedBy/Date批准者/日期")
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "ActualMH实际工时")
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "AccomplishedBy完工签署")
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "Station/Date航站/日期")
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "序号NUM")
                return MyTable.TYPE_TITLE_SMALL;
          /*else if (str == "件号P/N")
                return MyTable.TYPE_TITLE_SMALL;
            else if (str == "描述DESCRIPTION")
                return MyTable.TYPE_TITLE_SMALL;
            else if (str == "必需性NEC")
                return MyTable.TYPE_TITLE_SMALL;
            else if (str == "数量QT")
                return MyTable.TYPE_TITLE_SMALL;
            else if (str == "备注NOTE")
                return MyTable.TYPE_TITLE_SMALL;
            else if (str == "数量状态QTSTATUS")
                return MyTable.TYPE_TITLE_SMALL;*/
            else if (str == "图FIGURE")
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "PERFBY工作者" && GetUsedCount(str) == 1)
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "INSPBY检查者" && GetUsedCount(str) == 1)
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "工作步骤PROCEDURE")
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "工作卡反馈单JOBCARDFEEDBACKSHEET")
                return MyTable.TYPE_TITLE_DOWN;
            else if (str == "工作准备JOBSET-UP")
                return MyTable.TYPE_JOB_SET_UP;
            else if (str.IndexOf("1、注释Note") >= 0)
                return MyTable.TYPE_MIX;
            //else if (str.IndexOf("任选其一说明NOTEFORSELECTION") >= 0)
            //    return MyTable.TYPE_MIX;
            
            return MyTable.TYPE_CONTENT;
        }

        //删除文件夹或文件
        private void DeleteFile(string path)
        {
            FileAttributes attr = File.GetAttributes(path);
            if (attr == FileAttributes.Directory)
            {
                Directory.Delete(path, true);
            }
            else
            {
                File.Delete(path);
            }
        }

        //获取起点终点
        private float [] GetStartEndPoint(string strD)
        {
            //例strD = "M22.2 756.1L134.42 756.1L134.42 756.1L134.42 756.8199L134.42 756.8199L22.2 756.8199z";
            strD = strD.ToUpper();

            string[] strSplit = strD.Split('L');
            int nSplitCount = strSplit.Length;

            string strM = strSplit[0].Substring(1);

            strSplit[nSplitCount - 1] = strSplit[nSplitCount - 1].TrimEnd('Z');
            string[] strL = new string[nSplitCount-1];
            for (int i = 1; i < nSplitCount; i++)
            {
                strL[i-1] = strSplit[i];
            }
            
            string strMX = strM.Split(' ')[0];
            string strMY = strM.Split(' ')[1];
            float fMX = float.Parse(strMX);
            float fMY = float.Parse(strMY);

            float fDistance = 0;
            int nMaxPos = 0;
            for (int i = 0; i < strL.Length; i++ )
            {
                string strLXtemp = strL[i].Split(' ')[0];
                string strLYtemp = strL[i].Split(' ')[1];
                float fLXtemp = float.Parse(strLXtemp);
                float fLYtemp = float.Parse(strLYtemp);
                float fDistanceTemp = Math.Abs(fLXtemp - fMX);
                if (fDistanceTemp > fDistance)
                {
                    fDistance = fDistanceTemp;
                    nMaxPos = i;
                }

                fDistanceTemp = Math.Abs(fLYtemp - fMY);
                if (fDistanceTemp > fDistance)
                {
                    fDistance = fDistanceTemp;
                    nMaxPos = i;
                }

            }

            string strLX = strL[nMaxPos].Split(' ')[0];
            string strLY = strL[nMaxPos].Split(' ')[1];
            float fLX = float.Parse(strLX);
            float fLY = float.Parse(strLY);

            if (fMX > fLX)
            {
                float temp = fMX;
                fMX = fLX;
                fLX = temp;
            }

            if (fMY < fLY)
            {
                float temp = fMY;
                fMY = fLY;
                fLY = temp;
            }
            
            float[] retF = { fMX, fMY, fLX, fLY };
            return retF;
        }

        //获取文本
        private List<MyContent> GetText(HtmlDocument htmlDoc)
        {
            List<MyContent> list = new List<MyContent>();
            HtmlNodeCollection collection = htmlDoc.DocumentNode.SelectNodes("//text");
            if (collection == null)
                return null;
            foreach (HtmlNode node in collection)
            {
                MyContent content = new MyContent();
                string text = node.InnerText;
                //&#160;是一个空格
                if (text.IndexOf("&#160;") >= 0)
                {
                    text = text.Replace("&#160;", " ");
                }

                content.content = text;
                content.X = float.Parse(node.Attributes["X"].Value);
                content.Y = float.Parse(node.Attributes["Y"].Value);
                list.Add(content);
            }
            return list;
        }

        //获取图片
        private List<MyContent> GetImage(HtmlDocument htmlDoc)
        {
            List<MyContent> list = new List<MyContent>();
            HtmlNodeCollection collection = htmlDoc.DocumentNode.SelectNodes("//image");
            if (collection == null)
                return null;
            foreach (HtmlNode node in collection)
            {
                MyContent content = new MyContent();
                content.content = node.Attributes["xlink:href"].Value;

                string transform = node.SelectNodes("..")[0].Attributes["transform"].Value.TrimEnd(')');
                string [] strSpilt = transform.Split(' ');
                string strX = strSpilt[strSpilt.Length - 2];
                string strY = strSpilt[strSpilt.Length - 1];
                content.X = float.Parse(strX);
                content.Y = float.Parse(strY);
                list.Add(content);
            }

            return list;
        }

        //获取表格
        private List<MyTable> GetTable(HtmlDocument htmlDoc,int page)
        {
            List<MyTable> listTable = new List<MyTable>();

            //横线list
            List<MyTable> listHorizontal = new List<MyTable>();
            //竖线list
            List<MyTable> listVertical = new List<MyTable>();

            float fPageHeight = m_fPageHeights[page];
            
            HtmlNodeCollection collection = null;

            //两种找到"线"的标志
            HtmlNodeCollection collection1 = htmlDoc.DocumentNode.SelectNodes("//path[@stroke='#000000']");
            HtmlNodeCollection collection2 = htmlDoc.DocumentNode.SelectNodes("//path[@fill='#000000']");

            if (collection1 == null && collection2 == null)
            {
                collection = null;
            }
            else if (collection1 != null && collection2 == null)
            {
                collection = collection1;
            }
            else if (collection1 == null && collection2 != null)
            {
                collection = collection2;
            }
            else
            {
                if (collection1.Count >= collection2.Count)
                {
                    collection = collection1;
                }
                else
                {
                    collection = collection2;
                }
            }

            if (collection == null)
            {
                return listTable;
            }

            foreach (HtmlNode node in collection)
            {
                //d是线路径
                string strD = node.Attributes["d"].Value;
                //解析d
                float [] fPoint = GetStartEndPoint(strD);
               
                //M为起点 L为终点
                float nMX = fPoint[0];
                float nMY = fPoint[1];
                float nLX = fPoint[2];
                float nLY = fPoint[3];


                if (IsOneLine(nMY, nLY))
                {//Y相同是横线

                    if ( IsTooShort(nLX , nMX) )
                    {//去掉太短的
                        continue;
                    }

                    if (IsAlreadyInTable(listHorizontal, nMX, nMY, Math.Abs(nLX - nMX)))
                    {//去掉已存在的
                        continue;
                    }
                   
                    MyTable horizontal = new MyTable();
                    horizontal.X = nMX;
                    horizontal.Y = nMY;
                    horizontal.Width = Math.Abs(nLX - nMX);
                    horizontal.Height = 0;
                    listHorizontal.Add(horizontal);
                }
                else if (IsOneLine(nMX, nLX))
                {//X相同是竖线

                    if (IsTooShort(nMY, nLY))
                    {//去掉太短的
                        continue;
                    }

                    if (IsAlreadyInTable(listVertical, nMX, nMY, Math.Abs(nMY - nLY)))
                    {//去掉已存在的
                        continue;
                    }

                    MyTable vertical = new MyTable();
                    vertical.X = nMX;
                    vertical.Y = nMY;
                    vertical.Width = 0;
                    vertical.Height = Math.Abs(nMY - nLY);
                    listVertical.Add(vertical);
                }
            }

            //横线list和竖线list中有相同起始点的,判断为一个格子
            for (int i = 0; i < listHorizontal.Count; i++)
            {
                for (int j = 0; j < listVertical.Count; j++)
                {
                    if (IsOnePoint(listHorizontal[i].X, listHorizontal[i].Y, listVertical[j].X, listVertical[j].Y))
                    {
                        MyTable table = new MyTable();
                        table.X = listHorizontal[i].X;
                        table.Y = fPageHeight - listVertical[j].Y;
                        table.Width = listHorizontal[i].Width;
                        table.Height = listVertical[j].Height;
                        table.page = page;
                        listTable.Add(table);
                        break;
                    }
                }
            }
            return listTable;
        }


        //解析SVG文件
        private List<MyTable> SvgParse(string htmlPath, int page)
        {
            HtmlWeb htmlWeb = new HtmlWeb();
            htmlWeb.OverrideEncoding = Encoding.GetEncoding("utf-8");
            HtmlDocument htmlDoc = htmlWeb.Load(htmlPath);

            //表格list
            List<MyTable> listTable = GetTable(htmlDoc,page);

            if (listTable == null)
            {
                return null;
            }
            //文本list
            List<MyContent> listText = GetText(htmlDoc);
            
            //图像list
            List<MyContent> listImage = GetImage(htmlDoc);

            listTable.RemoveAt(3);//3是华夏航空图标表格,去掉

            //给表格横排竖列编号
            listTable[0].row = 0;
            listTable[0].col = 0;
            float Ytemp = listTable[0].Y;
            for (int i = 1; i < listTable.Count; i++)
            {
                if (IsOneLine(Ytemp, listTable[i].Y))
                {
                    listTable[i].row = listTable[i - 1].row;
                    listTable[i].col = listTable[i - 1].col + 1;
                }
                else
                {
                    listTable[i].row = listTable[i - 1].row + 1;
                    listTable[i].col = 0;
                    Ytemp = listTable[i].Y;
                }
            }

            //用来上下左右寻找表格用的list
            MyTable.m_listTables[page] = listTable;

            //内容填入表格(倒着数是因为有大表格套小表格,先把小表格填满,大表格就不填第二次了)
            for (int i = listTable.Count - 1; i >= 0; i--)
            {

                //右边没有表格的话补足横线长度
                if (listTable[i].Right() == null)
                {
                    listTable[i].Width = 710 - listTable[i].X;
                }

                //文本填入表格
                for (int j = 0; j < listText.Count; j++ )
                {
                    if (listText[j].IsUsed)
                    {
                        continue;
                    }
                    //判断是否在表格里
                    if(IsInTable( listTable[i],listText[j]))
                    {
                        listTable[i].Text += listText[j].content;
                        listText[j].IsUsed = true;
                    }
                }
                
                //图像填入表格
                for (int j = 0; j < listImage.Count; j++)
                {
                    //判断是否在表格里
                    if (IsInTable(listTable[i], listImage[j]))
                    {
                        listTable[i].Image = listImage[j].content;
                    }
                }

                //增加表格的类型
                listTable[i].Type = GetTableType(listTable[i].Text);
            }
                       
            return listTable;
        }


        private MyDictionary[] GetDictionarysFromString(string str)
        {
            //1、注释 Note：N/A2、特殊要求说明 Special Requirement：N/A3、改版说明 Revise Note：增加工作警戒提示信息Add maintenance caution information.
            MyDictionary[] dicts = new MyDictionary[3];
            int[] nPos = new int[4];
            nPos[3] = str.Length;
            for (int i = 3; i >= 1; i-- )
            {
                nPos[i - 1] = str.IndexOf(i.ToString() + "、");
                string strKeyValue = str.Substring(nPos[i - 1], nPos[i] - nPos[i - 1]);
                string[] strSplit = strKeyValue.Split('：');
                MyDictionary dict = new MyDictionary(strSplit[0], strSplit[1]);
                dicts[i - 1] = dict;
            }

            return dicts;
        }

        //listTable转dictionary
        private List<MyDictionary> ListTableToDictionary(List<MyTable> listTable)
        {
            List<MyDictionary> listDict = new List<MyDictionary>();

            StringBuilder sb = new StringBuilder();
            JsonWriter writer = new JsonWriter(sb);

            for (int i = 0; i < listTable.Count; i++)
            {
                string key = listTable[i].Text;
                int type = listTable[i].Type;
                if (type != MyTable.TYPE_CONTENT)
                {
                    MyTable tableContent = null;
                    if (type == MyTable.TYPE_TITLE_UP)
                    {
                        tableContent = listTable[i].Up();
                    }
                    else if (type == MyTable.TYPE_TITLE_DOWN)
                    {
                        tableContent = listTable[i].Down();
                    }
                    else if (type == MyTable.TYPE_TITLE_LEFT)
                    {
                        tableContent = listTable[i].Left();
                    }
                    else if (type == MyTable.TYPE_TITLE_RIGHT)
                    {
                        tableContent = listTable[i].Right();

                    }
                    else if (type == MyTable.TYPE_MIX)
                    {
                        MyDictionary [] dicts = GetDictionarysFromString(key);
                        for (int j = 0; j < dicts.Length; j++)
                        {
                            listDict.Add(dicts[j]);
                        }
                        continue;
                    }
                    else if (type == MyTable.TYPE_JOB_SET_UP)
                    {//工作准备
                        
                        string value = "";
                        tableContent = listTable[i].Down();
                        if (tableContent != null)
                        {
                            value += tableContent.Text;

                        }

                        for (int j = 1; ;j++ )
                        {
                            tableContent = listTable[i].Find(listTable[i].page + j, 2, listTable[i].col);
                            if (tableContent == null)
                            {
                                break;
                            }

                            if (tableContent.Text.Replace(" ", "") == "工作步骤PROCEDURE")
                            {
                                break;
                            }

                            value += tableContent.Text;

                        }

                        MyDictionary dist = new MyDictionary(key, value);
                        listDict.Add(dist);
                        
                        continue;
                        
                    }
                    else if (type == MyTable.TYPE_TITLE_SMALL)
                    {
                        List<MyTable> list = listTable[i].FindRow();
                        MyTable tableNum = list[0];
                        int RowsCount = 0;
                        while (true)
                        {
                            tableNum = tableNum.Down();
                            if (tableNum == null || !IsNumber(tableNum.Text))
                            {
                                break;
                            }
                            RowsCount++;

                        }

                        string smallKey = "";
                        string smallValue = "";
                        for (int j = 0; j < list.Count; j ++ )
                        {
                            smallKey += "|" + list[j].Text; 
                        }
                        smallKey = smallKey.Substring(1);

                        tableNum = list[0];
                        for (int j = 0; j < RowsCount; j++ )
                        {
                            string smallValueTemp = "";
                            tableNum = tableNum.Down();
                            foreach (MyTable t in tableNum.FindRow())
                            {
                                smallValueTemp += "|" + t.Text; 
                            }
                            smallValueTemp = smallValueTemp.Substring(1);

                            smallValue += "$" + smallValueTemp;
                        }
                        smallValue = smallValue.Substring(1);

                        MyDictionary dist = new MyDictionary(smallKey, smallValue);
                        listDict.Add(dist);

                        continue;

                    }

                    if (tableContent != null)
                    {
                        string value = tableContent.Text;
                        if (tableContent.Image != "")
                        {
                            value += "|" + tableContent.Image;
                        }
                       
                        MyDictionary dist = new MyDictionary(key, value);
                        listDict.Add(dist);
                    }
                }

            }

            return listDict;
        }

        //表格list转Dictionary之后再合并一些
        private List<MyDictionary> ListTableToDictionaryEx(List<MyTable> listTable)
        {
            List<MyDictionary> listDict = ListTableToDictionary(listTable);

            string key = "";
            string value = "";

            string [] values = new string [3];

            int keyPos = 0;
            int num = 0;
            for (int i = 0; i < listDict.Count; i++ )
            {
                if (listDict[i].Key.Replace(" ", "") == "工作准备JOBSET-UP")
                {
                    string valueTemp = listDict[i].Value;

                    string[] str = new string[4];

                    int nPos = valueTemp.LastIndexOf("III、");
                    str[3] = valueTemp.Substring(nPos);
                    valueTemp = valueTemp.Substring(0, nPos);

                    nPos = valueTemp.LastIndexOf("II、");
                    str[2] = valueTemp.Substring(nPos);
                    valueTemp = valueTemp.Substring(0, nPos);

                    nPos = valueTemp.LastIndexOf("I、");
                    str[1] = valueTemp.Substring(nPos);
                    str[0] = valueTemp.Substring(0, nPos);

                    key = listDict[i].Key;
                    value = str[0];
                    for(int j = 0; j < 3; j++)
                    {
                        values[j] = str[j + 1].Insert(str[j + 1].IndexOf("任选其一说明"), "：#");
                    }
                    listDict.RemoveAt(i);
                    keyPos = i;
                    i--;
                    
                }

                if (listDict[i].Key.Replace(" ", "").IndexOf("序号NUM") >= 0)
                {
                    num++;
                    string[] temp = values[num - 1].Split('#');
                    value += "@" + temp[0] + listDict[i].Key + "：" + listDict[i].Value + "#" + temp[1];
                    listDict.RemoveAt(i);
                    i--;
                }

            }

            listDict.Insert(keyPos,new MyDictionary(key,value));

            key = "";
            value = "";
            for (int i = 0; i < listDict.Count; i++)
            {
                if (listDict[i].Key.Replace(" ", "") == "工作步骤PROCEDURE")
                {
                    key = listDict[i].Key;
                    string [] arrayTemps = listDict[i].Value.Split('|');
                    string[] arrayValues = value.Split('|');

                    value = arrayValues[0] + arrayTemps[0];

                    for (int j = 1; j < arrayValues.Length; j++)
                    {
                        value += "|" + arrayValues[j];
                    }

                    for (int j = 1; j < arrayTemps.Length; j++)
                    {
                        value += "|" + arrayTemps[j];
                    }

                    listDict.RemoveAt(i);
                    keyPos = i;
                    i--;
                }
            }
            listDict.Insert(keyPos, new MyDictionary(key, value));


            return listDict;
        }


        //解析pdf
        private List<MyDictionary> PdfParse(string pdfPath)
        {
            int nPosPath = pdfPath.LastIndexOf('\\');
            int nPosPointer = pdfPath.ToLower().LastIndexOf(".pdf");
            string pdfName = pdfPath.Substring(nPosPath, nPosPointer - nPosPath);
            //创建一个缓存文件夹
            string tempPath = pdfPath.Substring(0, nPosPath) + "\\temp";
            string avgPath = pdfPath.Substring(0, nPosPath) + "\\temp" + pdfName + ".svg";
            if (!Directory.Exists(tempPath))
            {
                //创建文件夹
                Directory.CreateDirectory(tempPath);
            }

            //pdf转svg,返回总页数
            int nPageCount = PdfToSvg(pdfPath, avgPath);

            //表格list
            List<MyTable>[] listTables = new List<MyTable>[nPageCount];

            MyTable.m_listTables = new List<MyTable>[nPageCount];

            //解析SVG
            if (nPageCount == 1)
            {
                listTables[nPageCount - 1] = SvgParse(avgPath, nPageCount - 1);
            }
            else//nPageCount如果为0也没事
            {
                for (int i = 1; i <= nPageCount; i++)
                {
                    int nPos = avgPath.ToLower().IndexOf(".svg");
                    //页数大于1时,比如pdf文件名为e:\1.pdf,保存的svg文件名为e:\1_1.svg,e:\1_2.svg...解析需要自己改名,如下
                    string htmlPath = avgPath.Substring(0, nPos) + "_" + i.ToString() + ".svg";
                    listTables[i - 1] = SvgParse(htmlPath, i - 1);
                }
            }

            List<MyTable> listTableAll = new List<MyTable>();

            for(int i = 0; i < listTables.Length; i++)
            {
                for(int j = 0; j < listTables[i].Count; j++)
                {
                    listTableAll.Add(listTables[i][j]);
                }
            }
           
            List<MyDictionary> listDict = ListTableToDictionaryEx(listTableAll);

            DeleteFile(tempPath);

            return listDict;
        }


        private void button2_Click(object sender, RoutedEventArgs e)
        {

            string pdfPath = @"E:\pdfToSvg\1.pdf";

            List<MyDictionary> listDict = PdfParse(pdfPath);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < listDict.Count; i++)
            {
                sb.AppendLine("Key:" + listDict[i].Key + "--Value:" + listDict[i].Value);
            }
            File.WriteAllText(@"e:\1.txt", sb.ToString());
            
        }
        /*
        private void button3_Click(object sender, RoutedEventArgs e)
        {

            TOOLS_AND_SPARES_ obj = new TOOLS_AND_SPARES_();

            obj.TOOLS_AND_EQUIPMENTS = new TOOLS_AND_EQUIPMENTS_();
            obj.Expendables = new Expendables_();
            obj.Parts = new Parts_();

            obj.TOOLS_AND_EQUIPMENTS.TOOLS_AND_EQUIPMENTS = new List<TOOL_AND_EQUIPMENT_>();
            obj.Expendables.Expendables = new List<Expendable_>();
            obj.Parts.Parts = new List<Part_>();

            obj.TOOLS_AND_EQUIPMENTS.TOOLS_AND_EQUIPMENTS.Add(new TOOL_AND_EQUIPMENT_());
            obj.Expendables.Expendables.Add(new Expendable_());
            obj.Parts.Parts.Add(new Part_());

            string str = JsonMapper.ToJson(obj);
            MessageBox.Show(str);
            //string str = "\"name\": \"中国\", \"province\": [{\"name\": \"黑龙江\",\"cities\": {\"city\": [\"哈尔滨\", \"大庆\"]}}, {\"name\": \"广东\",\"cities\": {\"city\": [\"广州\", \"深圳\", \"珠海\"]}}, {\"name\": \"台湾\",\"cities\": {\"city\": [\"台北\", \"高雄\"]}}, {\"name\": \"新疆\",\"cities\": {\"city\": [\"乌鲁木齐\"]}}]";
           
            
            StringBuilder sb = new StringBuilder();
            JsonWriter writer = new JsonWriter(sb);

            writer.WriteObjectStart();

            writer.WritePropertyName("Name");
            writer.Write("yusong");

            writer.WritePropertyName("Age");
            writer.Write(26);

            writer.WritePropertyName("Girl");

            writer.WriteArrayStart();

            writer.WriteObjectStart();
            writer.WritePropertyName("name");
            writer.Write("ruoruo");
            writer.WritePropertyName("age");
            writer.Write(24);
            writer.WriteObjectEnd();

            writer.WriteObjectStart();
            writer.WritePropertyName("name");
            writer.Write("momo");
            writer.WritePropertyName("age");
            writer.Write(26);
            writer.WriteObjectEnd();

            writer.WriteArrayEnd();

            writer.WriteObjectEnd();

            string str = sb.ToString();
            MessageBox.Show(str);
            
            
            string htmlPath = @"E:\pdfToSvg\temp\1_3.svg";
            m_fPageHeights[2] = 842;
            List<MyTable> listDist = SvgParse(htmlPath, 2);
            
            
            StringBuilder sb = new StringBuilder();
            foreach (MyTable table in listTable)
            {
                sb.AppendLine(table.Text);
                //sb.AppendLine(table.X.ToString() + "|" + table.Y.ToString() + "|" + table.Width.ToString() + "|" + table.Height.ToString());
            }


            File.WriteAllText(@"e:\1.txt", sb.ToString());
            System.Diagnostics.Process.Start(@"e:\1.txt");
             
            //DeleteFile(@"E:\pdfToSvg\temp");

            //float[] fPageHeght = PdfToSvg(@"E:\pdfToSvg\1.pdf", @"E:\pdfToSvg\temp\1.svg");
        }
        */
    }
}
