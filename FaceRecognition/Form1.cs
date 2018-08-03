using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FaceRecognition
{
    public partial class Form1 : Form
    {
        //Declare varible
        MCvFont font = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_TRIPLEX,0.6d, 0.6d);
        HaarCascade faceDetected;
        Image<Bgr, Byte> Frame;
        Capture camera;
        Image<Gray, byte> result;
        Image<Gray, byte> TrainedFace = null;
        Image<Gray, byte> grayFace = null;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> Labels = new List<string>();
        List<string> users = new List<string>();
        int count, numLabels, t;
        string name, names = null;

       

        public Form1()
        {
            InitializeComponent();
            faceDetected = new HaarCascade("haarcascade_frontalface_default.xml");
            try
            {
                string labelA = File.ReadAllText(Application.StartupPath + "/Faces/Face.txt");
                string[] labels = labelA.Split(',');
                //first label will be nuber of faces saved
                numLabels = Convert.ToInt16(labels[0]);
                count = numLabels;
                string facesLoad;
                for (int i = 1; i < numLabels + 1; i++) {
                    facesLoad = "face " + i + ".bmp";
                    trainingImages.Add(new Image<Gray, byte>(Application.StartupPath + "/Faces/Face.txt"));
                    Labels.Add(labels[i]);
                } 
                 
            }
            catch (Exception ex) {
                MessageBox.Show("Nothing in the database");
            }

        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            camera = new Capture();
            camera.QueryFrame();
            Application.Idle += new EventHandler(FrameProcedure);
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            count = count + 1;
            grayFace = camera.QueryGrayFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            MCvAvgComp[][] detectedFaces = grayFace.DetectHaarCascade(faceDetected, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20,20));
            foreach (MCvAvgComp f in detectedFaces[0]) {
                TrainedFace = Frame.Copy(f.rect).Convert<Gray, byte>();
                break;
            }
            TrainedFace = result.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            trainingImages.Add(TrainedFace);
            Labels.Add(nameTxt.Text);
            File.WriteAllText(Application.StartupPath + "/Faces/Face.txt", trainingImages.ToArray().Length.ToString() + ",");
            for (int i = 1; i < trainingImages.ToArray().Length + 1; i++) {
                trainingImages.ToArray()[i - 1].Save(Application.StartupPath + "/Faces/face" + i + ".bmp");
                File.AppendAllText(Application.StartupPath + "/Faces/Face.txt", Labels.ToArray()[i - 1] + ",");
            }
            MessageBox.Show(nameTxt.Text + " Added Successfully to Database");
        }

        private void FrameProcedure(object sender, EventArgs e)
        {
            users.Add("");
            Frame = camera.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            grayFace = Frame.Convert<Gray, Byte>();
            MCvAvgComp[][] facesDetectedNow = grayFace.DetectHaarCascade(faceDetected, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));
            foreach (MCvAvgComp f in facesDetectedNow[0]) {
                result = Frame.Copy(f.rect).Convert<Gray,Byte>().Resize(100,100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                Frame.Draw(f.rect, new Bgr(Color.Green), 3);
                if (trainingImages.ToArray().Length != 0) {
                    MCvTermCriteria termCriteria = new MCvTermCriteria(count, 0.001);
                    EigenObjectRecognizer recognizer = new EigenObjectRecognizer(trainingImages.ToArray(), Labels.ToArray(), 1500, ref termCriteria);
                    name = recognizer.Recognize(result);
                    Frame.Draw(name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.Red));

                }
                
                users.Add("");
            }
            cameraBox.Image = Frame;
            names = "";
            users.Clear();
        }
    }
}
