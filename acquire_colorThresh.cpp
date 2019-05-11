/*
* starter_video.cpp
*
*  Created on: Nov 23, 2010
*      Author: Ethan Rublee
*
* A starter sample for using opencv, get a video stream and display the images
* easy as CV_PI right?
*/
#include "opencv2/imgproc/imgproc.hpp"
#include "opencv2/highgui/highgui.hpp"
#include <opencv2/features2d/features2d.hpp>
//#include <opencv2/objdetect/objdetect.hpp>
#include <iostream>
#include <vector>
#include <stdio.h>

using namespace cv;
using namespace std;

const char      
	* wndNameSrc = "Source",
	* wndNameFace = "Face",
	* wndNameTemplRes = "Template Match Res",
	* wndNameBin = "Binary",
	* wndNameBlobs = "Blobs",
	* wndNameLabels = "Labels";


//#define IMAGE_WIDTH 640
//#define IMAGE_HEIGHT 480

//hide the local functions in an anon namespace
namespace {
  struct CV_EXPORTS Center
  {
      Point2d location;
      double radius;
      double confidence;
  };
	//
	// http://nghiaho.com/?p=1102
	//
	void FindBlobs(const cv::Mat &binary, vector < vector<cv::Point2i>  > &blobs)
	{
		blobs.clear();

		// Fill the label_image with the blobs
		// 0  - background
		// 1  - unlabelled foreground
		// 2+ - labelled foreground

		cv::Mat label_image;
		binary.convertTo(label_image, CV_32FC1); // weird it doesn't support CV_32S!

		int label_count = 2; // starts at 2 because 0,1 are used already

		for(int y=0; y < binary.rows; y++)
		{
			for(int x=0; x < binary.cols; x++)
			{
				//if((int)label_image.at(y,x) != 1)
				if((int)label_image.data[y*binary.cols+x] < 1)
				{
					continue;
				}

				cv::Rect rect;
				cv::floodFill(label_image, cv::Point(x,y), cv::Scalar(label_count), 
					&rect, cv::Scalar(0), cv::Scalar(0), 4);

				std::vector<Point2i>  blob;

				for(int i=rect.y; i < (rect.y+rect.height); i++)
				{
					for(int j=rect.x; j < (rect.x+rect.width); j++)
					{
						//if((int)label_image.at(i,j) != label_count)
						if((int)label_image.data[i*rect.width+j] != label_count)
						{
							continue;
						}

						blob.push_back(cv::Point2i(j,i));
					}
				}

				blobs.push_back(blob);

				label_count++;
			}
		}
	}
/*
	//
	// http://code.google.com/p/my-masters-thesis/source/browse/trunk/MastersThesis/FacialFeatures/FacialFeatures.cpp?spec=svn66&r=66
	//
	void BlobDetector( Mat org, Mat src )
	{
		Mat out;
		vector<KeyPoint> keyPoints;
		vector <vector <Point>> contours,
			approxContours;
		vector<Center> centerPoints;

		SimpleBlobDetector::Params params;
		params.minThreshold = 50;
		params.maxThreshold = 100;
		params.thresholdStep = 5;

		params.minArea = 100; 
		params.minConvexity = 0.3;
		params.minInertiaRatio = 0.01;

		params.maxArea = 8000;
		params.maxConvexity = 10;
#if 0
		params.filterByArcLength = true;
		params.minArcLen = 100;
		params.maxArcLen = 400;
#endif
		params.filterByColor = false;
		params.filterByCircularity = false;

		vector<Center>  blobCenter;			//+

		SimpleBlobDetector blobs( params );
		blobs.create("SimpleBlob");

		//blobs.detectEx( src, keyPoints, contours, Mat() );
		blobs.detect( src, keyPoints );
		//blobs.findBlobs(org, src, blobCenter);
		
		drawKeypoints( src, keyPoints, out, CV_RGB(0,255,0), DrawMatchesFlags::DEFAULT );
		approxContours.resize( contours.size() );

		for( int i = 0; i < contours.size(); ++i )
		{
			approxPolyDP( Mat(contours[i]), approxContours[i], 4, 1 );
			drawContours( out, contours, i, CV_RGB(rand()&255, rand()&255, rand()&255) );
			drawContours( out, approxContours, i, CV_RGB(rand()&255, rand()&255, rand()&255) );
		}
		cout << "DEBUG Keypoints " << keyPoints.size() << endl;

		imshow( wndNameBlobs, out );
		//waitKey(0);
	}
*/
    void help(char** av) {
        cout << "\nThis program justs gets you started reading images from video\n"
            "Usage:\n./" << av[0] << " <video device number>\n"
            << "q,Q,esc -- quit\n"
            << "space   -- save frame\n\n"
            << "\tThis is a starter sample, to get you up and going in a copy pasta fashion\n"
            << "\tThe program captures frames from a camera connected to your computer.\n"
            << "\tTo find the video device number, try ls /dev/video* \n"
            << "\tYou may also pass a video file, like my_vide.avi instead of a device number"
            << endl;
    }

    int process(VideoCapture& capture) {
    	int n = 0;
    	char filename[200];
        string window_name = "video | q or esc to quit";
		string convWin_name = "HSV";
		string convWin2_name = "HSV2";
		string srcWin_name = "RGB";
		string dstWin_name = "RGB thresh";
        cout << "press space to save a picture. q or esc to quit" << endl;
        //namedWindow(window_name, CV_WINDOW_KEEPRATIO); //resizable window;
		Mat frame, frame_rgb;
		Mat frame_gray;
		Mat frame_hsv, frame_hsv2;
		Mat img_h, img_s, img_v;

		int hue_=20., sat_=200., val_=170.;
		int hue_low = 10.;
		int r_= 10, g_ = 10, b_ = 10;
		string tbarname1 = "Hue";
		string tbarname1low = "HueLow";
		string tbarname2 = "Saturation";
		string tbarname3 = "Value";
		namedWindow(window_name, 1);
		createTrackbar(tbarname1, window_name, &hue_, 359);
		createTrackbar(tbarname1low, window_name, &hue_low, 300);
		createTrackbar(tbarname2, window_name, &sat_, 255);
		createTrackbar(tbarname3, window_name, &val_, 255);
		namedWindow(dstWin_name, 1);
		createTrackbar("R thresh", dstWin_name, &r_, 255);
		createTrackbar("G thresh", dstWin_name, &g_, 255);
		createTrackbar("B thresh", dstWin_name, &b_, 255);

        capture >> frame;
		int width = frame.cols, height = frame.rows;
		Mat img = Mat::zeros(Size(width,height), CV_8UC1);
		Mat output = Mat::zeros(Size(width,height), CV_8UC1);
		Mat binary;
		Mat imgRgb = Mat::zeros(Size(width,height), CV_8UC1);

        for (;;) {
            capture >> frame;
            if (frame.empty())
                continue;

			/// Convert it to gray
			cvtColor( frame, frame_gray, CV_RGB2GRAY );
			cvtColor( frame, frame_hsv, CV_BGR2HSV );
			//cvtColor( frame, frame_hsv2, CV_RGB2HSV );
			cvtColor( frame, frame_rgb, CV_RGB2BGR );
			int ch = frame_hsv.channels(); // =3
			int gch = img.channels(); // =1
			int chRgb = frame_rgb.channels(); // =3
			int gchRgb = imgRgb.channels(); // =1
			for(int y=0; y<height; ++y)
			{
				uchar *p = frame_hsv.ptr(y);
				uchar *q = img.ptr(y);
				uchar *u = frame_rgb.ptr(y);
				uchar *v = imgRgb.ptr(y);
				for(int x=0; x<width; ++x)
				{
					//uchar valH = p[x*ch+0] < 18 ? UCHAR_MAX : 0;
					//uchar valH = p[x*ch+0] < hue_ ? UCHAR_MAX : 0;
					uchar valH = p[x*ch+0] < hue_ && p[x*ch+0] > hue_low ? UCHAR_MAX : 0;
					uchar valS = p[x*ch+1] < sat_ ? UCHAR_MAX : 0;
					uchar valV = p[x*ch+2] < val_ ? UCHAR_MAX : 0;
					//q[x*gch] = p[x*ch+2] > 50 ? p[x*ch+2] : 0;
					//q[x*gch] = valH+valS+valV > 0 ? UCHAR_MAX : 0;
					q[x*gch] = valH & valS & valV;

					uchar valR = u[x*chRgb+0] < r_ ? UCHAR_MAX : 0;
					uchar valG = u[x*chRgb+1] < g_ ? UCHAR_MAX : 0;
					uchar valB = u[x*chRgb+2] < b_ ? UCHAR_MAX : 0;
					v[x*gchRgb] = valR & valG & valB;
				}
			}
#if 1

			threshold(frame_gray, binary, g_, 255, THRESH_BINARY);
			imshow( wndNameBin, binary );
/*			vector <vector<cv::Point2i>> blobs;
			FindBlobs(binary, blobs);
			// Randomy color the blobs
			for(size_t i=0; i < blobs.size(); i++)
			{
				unsigned char r = 255 * (rand()/(1.0 + RAND_MAX));
				unsigned char g = 255 * (rand()/(1.0 + RAND_MAX));
				unsigned char b = 255 * (rand()/(1.0 + RAND_MAX));

				for(size_t j=0; j < blobs[i].size(); j++)
				{
					int x = blobs[i][j].x;
					int y = blobs[i][j].y;

					//output.at(y,x)[0] = b;
					//output.at(y,x)[1] = g;
					//output.at(y,x)[2] = r;
					 int a = output.step*y+(x*ch);
					output.data[a+0] = b;
					output.data[a+1] = g;
					output.data[a+2] = r;
				}
			}
			imshow( wndNameLabels, output );
*/
#else
			BlobDetector(frame_gray);
#endif

			/// Create window
			//namedWindow( window_name, CV_WINDOW_AUTOSIZE );
			imshow( srcWin_name, frame );
			imshow( convWin_name, frame_hsv );
			//imshow( convWin2_name, frame_hsv2 );
			imshow( dstWin_name, imgRgb );
			imshow( window_name, img );
             char key = (char)waitKey(5); //delay N millis, usually long enough to display and capture input
            switch (key) {
        case 'q':
        case 'Q':
        case 27: //escape key
            return 0;
        case ' ': //Save an image
        	sprintf(filename,"filename%.3d.jpg",n++);
        	imwrite(filename,frame);
        	cout << "Saved " << filename << endl;
        	break;
        default:
            break;
            }
        }
        return 0;
    }

}

//int main(int ac, char** av)
int main()
{
    //if (ac != 2) {
    //    help(av);
    //    return 1;
    //}
    //std::string arg = av[1];
	std::string arg = "2";
    VideoCapture capture(arg); //try to open string, this will attempt to open it as a video file
    if (!capture.isOpened()) //if this fails, try to open as a video camera, through the use of an integer param
        capture.open(2);
    if (!capture.isOpened()) {
        cerr << "Failed to open a video device or video file!\n" << endl;
        //help(av);
        return 1;
    }
    return process(capture);
}

