// testTable.cpp : 定义控制台应用程序的入口点。
//

#include "stdafx.h"
#include <opencv2/opencv.hpp>
#include <vector>
using namespace std;
using namespace cv;
//#include <afx.h>
#include <Windows.h>


//切割字符串
vector<string> split(const string& str, const string& delim) {
	vector<string> res;
	if("" == str) return res;
	//先将要切割的字符串从string类型转换为char*类型
	char * strs = new char[str.length() + 1] ; //不要忘了
	strcpy(strs, str.c_str()); 

	char * d = new char[delim.length() + 1];
	strcpy(d, delim.c_str());

	char *p = strtok(strs, d);
	while(p) {
		string s = p; //分割得到的字符串转换为string类型
		res.push_back(s); //存入结果数组
		p = strtok(NULL, d);
	}

	return res;
}

int _tmain(int argc, _TCHAR* argv[])
{
	char szPath[] = "e:\\1.ini";

	for (int i = 0; i < 8; i++)
	{
		//Mat mat = imread("e:\\1.png");
		Mat mat = Mat::zeros(Size(900, 900), CV_8UC3);
		//设置白色背景
		mat.setTo(Scalar(255,255,255));

		char szPage[16] = {0};
		sprintf(szPage,"page%d",i);
		char szCount[16] = {0};
		GetPrivateProfileString(szPage,"count","0",szCount,16,szPath);
		int count = atoi(szCount);
		for (int j = 0; j < count; j++)
		{
			
			char szTableNum[16] = {0};
			sprintf(szTableNum,"table%d",j);
			char szTable[64] = {0};
			GetPrivateProfileString(szPage,szTableNum,"",szTable,64,szPath);

			string strTable(szTable);
			if (strTable == "")
			{
				continue;
			}
			vector<string> strSplit = split(strTable,"|");

			int x = atoi(strSplit[0].c_str());
			int y = atoi(strSplit[1].c_str());
			int w = atoi(strSplit[2].c_str());
			int h = atoi(strSplit[3].c_str());
			//int row = atoi(strSplit[4].c_str());
			//int col = atoi(strSplit[5].c_str());

			rectangle(mat, Rect(x, y, w, h), Scalar(0, 255, 0),2);

			string text = strSplit[4] + "," + strSplit[5];
			int font_face = FONT_HERSHEY_COMPLEX_SMALL;
			double font_scale = 0.5;
			int thickness = 1;
			int baseline;  //获取文本框的长宽  
			Size text_size = getTextSize(text, font_face, font_scale, thickness, &baseline);    //将文本框居中绘制  
			Point origin;   
			origin.x = x + w / 2 - text_size.width / 2;  
			origin.y = y + h / 2 + text_size.height / 2;  
			putText(mat, text, origin, font_face, font_scale, Scalar(0, 0, 255));
			
		}
		//MessageBox(NULL,"1",NULL,NULL);
		//namedWindow("1");
		imshow("1",mat);
		waitKey();
		//
		//system("pause");
	}

	
	
	return 0;
}

