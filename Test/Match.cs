using OpenCvSharp.Features2D;
using OpenCvSharp.XFeatures2D;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Test
{
    class SiftSurfSample
    {
        public void RunTest(string str1, string str2, string str3, string str4)
        {
            using var src1 = new Mat(str1, ImreadModes.Color);
            using var src2 = new Mat(str2, ImreadModes.Color);
            using var src3 = new Mat(str3, ImreadModes.Color);
            using var src4 = new Mat(str4, ImreadModes.Color);
            Cv2.ImShow("src1", src1);
            Cv2.ImShow("src2", src2);
            //MatchBySift(src1, src2);
            //MatchBySurf(src1, src2);
            Match(src1, src2);
            StitcherMat(new List<Mat>() { src1, src2}, out string msg);
        }
        public Mat StitcherMat(List<Mat> images, out string errMsg)
        {
            var stitcher = Stitcher.Create(Stitcher.Mode.Scans);
            var pano = new Mat();
            var status = stitcher.Stitch(images, pano);
            if (status != Stitcher.Status.OK)
            {
                errMsg = "失败：" + status.ToString();
                return null;
            }
            errMsg = "";
            Cv2.ImShow("pano", pano);
            Cv2.WaitKey();
            return pano;
        }
        public void MatchBySift(Mat src1, Mat src2)
        {
            using var gray1 = new Mat();
            using var gray2 = new Mat();

            Cv2.CvtColor(src1, gray1, ColorConversionCodes.BGR2GRAY);
            Cv2.CvtColor(src2, gray2, ColorConversionCodes.BGR2GRAY);

            using var sift = SIFT.Create();

            // Detect the keypoints and generate their descriptors using SIFT
            using var descriptors1 = new Mat<float>();
            using var descriptors2 = new Mat<float>();
            sift.DetectAndCompute(gray1, null, out var keypoints1, descriptors1);
            sift.DetectAndCompute(gray2, null, out var keypoints2, descriptors2);

            // Match descriptor vectors
            using var bfMatcher = new BFMatcher(NormTypes.L2, false);
            using var flannMatcher = new FlannBasedMatcher();
            DMatch[] bfMatches = bfMatcher.Match(descriptors1, descriptors2);
            DMatch[] flannMatches = flannMatcher.Match(descriptors1, descriptors2);

            // Draw matches
            using var bfView = new Mat();
            Cv2.DrawMatches(gray1, keypoints1, gray2, keypoints2, bfMatches, bfView);
            using var flannView = new Mat();
            Cv2.DrawMatches(gray1, keypoints1, gray2, keypoints2, flannMatches, flannView);
            Cv2.ImShow("SIFT matching (by BFMather)", bfView);
            Cv2.ImShow("SIFT matching (by flannView)", flannView);
           

        }
        public void Match(Mat img1,Mat img2) 
        {
            int w = img2.Width;
            int h = img2.Height;

            Mat img0 = new Mat(new Size(w * 2, h * 2), img2.Type());

            img0[0, h, w, w + w] = img2;

            ORB orb = ORB.Create(10000);
            Mat dscrip1 = new Mat();
            Mat dscrip2 = new Mat();
            orb.DetectAndCompute(img1, null, out KeyPoint[] keyPoint1, dscrip1);
            orb.DetectAndCompute(img0, null, out KeyPoint[] keyPoint2, dscrip2);
            //  暴力匹配
            BFMatcher matcher = new BFMatcher(NormTypes.L1, false);
            DMatch[] match = matcher.Match(dscrip1, dscrip2);

            match = match.OrderBy(x => x.Distance).ToArray();
            var goodmatch = match.Take(1600);

            if (goodmatch.Count() < 4) return;

            //画出匹配关系
            Mat outImg = new Mat();
            Cv2.DrawMatches(img1, keyPoint1, img0, keyPoint2, goodmatch, outImg, flags: DrawMatchesFlags.DrawRichKeypoints | DrawMatchesFlags.NotDrawSinglePoints);
            Cv2.ImShow("ORB", outImg);

            // 提取匹配的位置
            var pointsSrc = new List<Point2f>();
            var pointsDst = new List<Point2f>();
            foreach (var m in goodmatch)
            {
                pointsSrc.Add(keyPoint1[m.QueryIdx].Pt);
                pointsDst.Add(keyPoint2[m.TrainIdx].Pt);
            }
            List<Point2d> pSrc=new List<Point2d>() ;
            List<Point2d> pDst = new List<Point2d>();
            foreach (var item in pointsSrc) 
            {
                pSrc.Add(Point2FToPoint2D(item));
            }
            foreach (var item in pointsDst)
            {
                pDst.Add(Point2FToPoint2D(item));
            }

            //获得变换矩阵
            var M = Cv2.FindHomography ( pSrc, pDst, HomographyMethods.Ransac);
            Console.WriteLine(M);
            Console.WriteLine(Cv2.Format(M));

            // 对 img1 透视变换
            var result = new Mat();
            Cv2.WarpPerspective(img1, result, M, new Size(w * 2, h * 2));

            // 将img2拼接到结果
            result[0, h, w, w + w] = img2;
            Cv2.ImShow("img2", result);
            Cv2.WaitKey();
        }

        protected Point2d Point2FToPoint2D(Point2f input)
        {
            return new Point2d(input.X, input.Y);
        }

        public void MatchBySurf(Mat src1, Mat src2)
        {
            using var gray1 = new Mat();
            using var gray2 = new Mat();

            Cv2.CvtColor(src1, gray1, ColorConversionCodes.BGR2GRAY);
            Cv2.CvtColor(src2, gray2, ColorConversionCodes.BGR2GRAY);

            using var surf = SURF.Create(200, 4, 2, true);

            // Detect the keypoints and generate their descriptors using SURF
            using var descriptors1 = new Mat<float>();
            using var descriptors2 = new Mat<float>();
            surf.DetectAndCompute(gray1, null, out var keypoints1, descriptors1);
            surf.DetectAndCompute(gray2, null, out var keypoints2, descriptors2);

            // Match descriptor vectors 
            using var bfMatcher = new BFMatcher(NormTypes.L2, false);
            using var flannMatcher = new FlannBasedMatcher();
            DMatch[] bfMatches = bfMatcher.Match(descriptors1, descriptors2);
            DMatch[] flannMatches = flannMatcher.Match(descriptors1, descriptors2);

            // Draw matches
            using var bfView = new Mat();
            Cv2.DrawMatches(gray1, keypoints1, gray2, keypoints2, bfMatches, bfView);
            using var flannView = new Mat();
            Cv2.DrawMatches(gray1, keypoints1, gray2, keypoints2, flannMatches, flannView);
            Cv2.ImShow("SURF matching (by BFMather)", bfView);
            Cv2.ImShow("SURF matching (by FlannBasedMatcher)", flannView);
            
            Cv2.WaitKey();

        }

    }

}
