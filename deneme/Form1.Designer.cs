using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace validator
{
    partial class Form1
    {

        private System.ComponentModel.IContainer components = null;


        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            txtXmlPath = new TextBox();
            btnBrowseXml = new Button();
            txtXsdHeadPath = new TextBox();
            btnBrowseXsdHead = new Button();
            txtXsdPacsPath = new TextBox();
            btnBrowseXsdPacs = new Button();
            btnValidate = new Button();
            txtResults = new RichTextBox();
            label4 = new Label();
            label5 = new Label();
            richTextBox1 = new RichTextBox();
            label1 = new Label();
            btnEdıtor = new Button();
            btnSaveChanges = new Button();
            btnViewUpdatedXml_Click = new Button();
            errorProvider1 = new ErrorProvider(components);
            dataGridViewXml = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)errorProvider1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewXml).BeginInit();
            SuspendLayout();
            // 
            // txtXmlPath
            // 
            txtXmlPath.Location = new Point(22, 26);
            txtXmlPath.Margin = new Padding(6);
            txtXmlPath.Name = "txtXmlPath";
            txtXmlPath.Size = new Size(868, 39);
            txtXmlPath.TabIndex = 1;
            // 
            // btnBrowseXml
            // 
            btnBrowseXml.Location = new Point(902, 26);
            btnBrowseXml.Margin = new Padding(6);
            btnBrowseXml.Name = "btnBrowseXml";
            btnBrowseXml.Size = new Size(223, 44);
            btnBrowseXml.TabIndex = 2;
            btnBrowseXml.Text = "XML";
            btnBrowseXml.UseVisualStyleBackColor = true;
            btnBrowseXml.Click += btnBrowseXml_Click;
            // 
            // txtXsdHeadPath
            // 
            txtXsdHeadPath.Location = new Point(22, 87);
            txtXsdHeadPath.Margin = new Padding(6);
            txtXsdHeadPath.Name = "txtXsdHeadPath";
            txtXsdHeadPath.Size = new Size(868, 39);
            txtXsdHeadPath.TabIndex = 4;
            // 
            // btnBrowseXsdHead
            // 
            btnBrowseXsdHead.Location = new Point(902, 82);
            btnBrowseXsdHead.Margin = new Padding(6);
            btnBrowseXsdHead.Name = "btnBrowseXsdHead";
            btnBrowseXsdHead.Size = new Size(223, 50);
            btnBrowseXsdHead.TabIndex = 5;
            btnBrowseXsdHead.Text = "Header XSD";
            btnBrowseXsdHead.UseVisualStyleBackColor = true;
            btnBrowseXsdHead.Click += btnBrowseXsdHead_Click;
            // 
            // txtXsdPacsPath
            // 
            txtXsdPacsPath.Location = new Point(22, 149);
            txtXsdPacsPath.Margin = new Padding(6);
            txtXsdPacsPath.Name = "txtXsdPacsPath";
            txtXsdPacsPath.Size = new Size(868, 39);
            txtXsdPacsPath.TabIndex = 7;
            // 
            // btnBrowseXsdPacs
            // 
            btnBrowseXsdPacs.Location = new Point(902, 149);
            btnBrowseXsdPacs.Margin = new Padding(6);
            btnBrowseXsdPacs.Name = "btnBrowseXsdPacs";
            btnBrowseXsdPacs.Size = new Size(223, 44);
            btnBrowseXsdPacs.TabIndex = 8;
            btnBrowseXsdPacs.Text = "Document XSD";
            btnBrowseXsdPacs.UseVisualStyleBackColor = true;
            btnBrowseXsdPacs.Click += btnBrowseXsdPacs_Click;
            // 
            // btnValidate
            // 
            btnValidate.Location = new Point(22, 221);
            btnValidate.Margin = new Padding(6);
            btnValidate.Name = "btnValidate";
            btnValidate.Size = new Size(868, 53);
            btnValidate.TabIndex = 9;
            btnValidate.Text = "Doğrula";
            btnValidate.UseVisualStyleBackColor = true;
            btnValidate.Click += btnValidate_Click;
            // 
            // txtResults
            // 
            txtResults.BackColor = SystemColors.Window;
            txtResults.Location = new Point(22, 836);
            txtResults.Margin = new Padding(6);
            txtResults.Name = "txtResults";
            txtResults.ReadOnly = true;
            txtResults.Size = new Size(1103, 528);
            txtResults.TabIndex = 11;
            txtResults.Text = "";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(22, 282);
            label4.Margin = new Padding(6, 0, 6, 0);
            label4.Name = "label4";
            label4.Size = new Size(215, 32);
            label4.TabIndex = 11;
            label4.Text = "XML Görüntüleyici:";
            // 
            // label5
            // 
            label5.Location = new Point(0, 0);
            label5.Name = "label5";
            label5.Size = new Size(100, 23);
            label5.TabIndex = 19;
            // 
            // richTextBox1
            // 
            richTextBox1.Location = new Point(22, 331);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(1103, 464);
            richTextBox1.TabIndex = 17;
            richTextBox1.Text = "";
            richTextBox1.WordWrap = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(22, 798);
            label1.Name = "label1";
            label1.Size = new Size(111, 32);
            label1.TabIndex = 21;
            label1.Text = "Sonuçlar:";
            label1.Click += label1_Click;
            // 
            // btnEdıtor
            // 
            btnEdıtor.Location = new Point(902, 221);
            btnEdıtor.Name = "btnEdıtor";
            btnEdıtor.Size = new Size(233, 53);
            btnEdıtor.TabIndex = 22;
            btnEdıtor.Text = "XML Editöre Yükle";
            btnEdıtor.UseVisualStyleBackColor = true;
            btnEdıtor.Click += btnEditor_Click;
            // 
            // btnSaveChanges
            // 
            btnSaveChanges.BackColor = SystemColors.ActiveBorder;
            btnSaveChanges.Location = new Point(1777, 1339);
            btnSaveChanges.Name = "btnSaveChanges";
            btnSaveChanges.Size = new Size(150, 46);
            btnSaveChanges.TabIndex = 23;
            btnSaveChanges.Text = "Kaydet";
            btnSaveChanges.UseVisualStyleBackColor = false;
            // 
            // btnViewUpdatedXml_Click
            // 
            btnViewUpdatedXml_Click.BackColor = SystemColors.ActiveBorder;
            btnViewUpdatedXml_Click.Location = new Point(1945, 1339);
            btnViewUpdatedXml_Click.Name = "btnViewUpdatedXml_Click";
            btnViewUpdatedXml_Click.Size = new Size(287, 46);
            btnViewUpdatedXml_Click.TabIndex = 24;
            btnViewUpdatedXml_Click.Text = "Güncelenen Xml'i Gör";
            btnViewUpdatedXml_Click.UseVisualStyleBackColor = false;
            // 
            // errorProvider1
            // 
            errorProvider1.ContainerControl = this;
            // 
            // dataGridViewXml
            // 
            dataGridViewXml.AllowUserToAddRows = false;
            dataGridViewXml.AllowUserToDeleteRows = false;
            dataGridViewXml.AllowUserToResizeRows = false;
            dataGridViewXml.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewXml.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewXml.BackgroundColor = SystemColors.Window;
            dataGridViewXml.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewXml.Location = new Point(1170, 26);
            dataGridViewXml.Name = "dataGridViewXml";
            dataGridViewXml.RowHeadersWidth = 51;
            dataGridViewXml.RowTemplate.Height = 29;
            dataGridViewXml.Size = new Size(1082, 1298);
            dataGridViewXml.TabIndex = 27;
            dataGridViewXml.Visible = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(2326, 1379);
            Controls.Add(dataGridViewXml);
            Controls.Add(btnViewUpdatedXml_Click);
            Controls.Add(btnSaveChanges);
            Controls.Add(btnEdıtor);
            Controls.Add(label1);
            Controls.Add(richTextBox1);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(txtResults);
            Controls.Add(btnValidate);
            Controls.Add(btnBrowseXsdPacs);
            Controls.Add(txtXsdPacsPath);
            Controls.Add(btnBrowseXsdHead);
            Controls.Add(txtXsdHeadPath);
            Controls.Add(btnBrowseXml);
            Controls.Add(txtXmlPath);
            Margin = new Padding(6);
            Name = "Form1";
            Text = "XML/XSD Validasyon Uygulaması";
            ((System.ComponentModel.ISupportInitialize)errorProvider1).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewXml).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtXmlPath;
        private Button btnBrowseXml;
        private TextBox txtXsdHeadPath;
        private Button btnBrowseXsdHead;
        private TextBox txtXsdPacsPath;
        private Button btnBrowseXsdPacs;
        private Button btnValidate;
        private RichTextBox txtResults;
        private Label label4;
        private Label label5;
        private RichTextBox richTextBox1;
        private Label label1;
        private Button btnEdıtor;
        private Button btnSaveChanges;
        private Button btnViewUpdatedXml_Click;

        private ErrorProvider errorProvider1;

        private DataGridView dataGridViewXml;
    }
}
