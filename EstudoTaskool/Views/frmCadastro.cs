﻿using EstudoTaskool.Database;
using EstudoTaskool.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EstudoTaskool
{
    public partial class frmCadastro : Form
    {
        private Usuario usuario = null;
        bool isValidEmail;

        public frmCadastro()
        {
            InitializeComponent();
        }

        public frmCadastro(Usuario usuario)
        {
            InitializeComponent();

            this.usuario = usuario;
        }

        private void frmCadastro_Load(object sender, EventArgs e)
        {
            if (usuario != null)
            {
                nomeTextBox.Text = usuario.Nome;
                emailTextBox.Text = usuario.Email;
                telefoneTextBox.Text = usuario.Telefone;
                usuarioTextBox.Text = usuario.Username;

                if (usuario.Foto != null)
                {
                    MemoryStream ms = new MemoryStream(usuario.Foto);
                    fotoPictureBox.Image = Image.FromStream(ms); 
                }
            }
        }

        private void btnGerarAleatorio_Click(object sender, EventArgs e)
        {
            //verifica se tem algo digitado no campo
            if (nomeTextBox.Text.Trim().Length == 0)
            {
                MessageBox.Show("Digite um nome para gerar aleatório", "Erro");
                return;
            }
            //verifica se tem o caracter espaço - tem 2 nomes digitados
            if (nomeTextBox.Text.Split(' ').Length == 1)
            {
                MessageBox.Show("Não foi possível gerar aleatório.", "Erro");
            }
            else
            {
                //guarda o conteúdo da caixa de texto
                string input = nomeTextBox.Text;
                //pega a posição do espaço
                int posEspaco = input.IndexOf(" ", 0);

                //separa as duas parte
                string parte1 = input.Substring(0, posEspaco);
                string parte2 = input.Substring(posEspaco + 1, input.Length - (posEspaco + 1));
                //junta o 1o nome + '.' + 2o nome + 2 digitos do ano
                string userName = parte1 + "." + parte2 + dtpNascimento.Value.ToString("yy").ToString();
                usuarioTextBox.Text = userName;

            }
        }

        private void Inputs_Leave(object sender, EventArgs e)
        {

            //exconde a borda do controle
            pictureBox1.Visible = false;

            Control c = sender as Control;
            if (c.GetType().ToString() == "System.Windows.Forms.DateTimePicker")
            {
                DateTimePicker dtp = sender as DateTimePicker;
                // calcula a idade
                //Verifica se a data atual é maior que a data do aniversario
                //value = conteudo selecionado no datetimepicker
                //now é a data do sistema
                if (dtp.Value < DateTime.Now)
                {
                    //chama a função que calcula a idade (F12 - vai p a função)
                    int resultado = CalculaIdade(dtp.Value);
                    lblIdade.Text = $"{resultado} anos";
                }
            }

            // Outra forma não tão elegante
            //DateTimePicker dtp1 = sender as DateTimePicker;
            //if (dtp1 != null)
            //{

            //}


        }

        private void Inputs_Enter(object sender, EventArgs e)
        {

            TextBox tbx = sender as TextBox;
            if (tbx != null)
            {
                pictureBox1.Location = new Point(tbx.Location.X - 5, tbx.Location.Y - 5);
                pictureBox1.Size = new Size(tbx.Size.Width + 10, tbx.Size.Height + 10);
                pictureBox1.Visible = true;
            }
            else
            {

                DateTimePicker dtp = sender as DateTimePicker;

                if (dtp != null)
                {
                    pictureBox1.Location = new Point(dtp.Location.X - 5, dtp.Location.Y - 5);
                    pictureBox1.Size = new Size(dtp.Size.Width + 10, dtp.Size.Height + 10);
                    pictureBox1.Visible = true;
                }

            }

        }

        private static int CalculaIdade(DateTime dataNascimento)
        {
            //pega a parte ano das datas e subtrai
            int idade = DateTime.Now.Year - dataNascimento.Year;
            //verifica se i dia é anterior a data atual
            if (DateTime.Now.DayOfYear < dataNascimento.DayOfYear)
            {
                idade = idade - 1;
            }

            return idade;
        }

        private void btnSelecionar_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            //O filtro - somente apresentar os arquivos de imagem
            ofd.Filter = "Arquivos de imagens(*.jpg;*.png)|*.jpg;*.png";
            //Chamar a janela - entrar somente se confirma a seleção do arquivo
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                //Carregar a imagem selecionada no picturebox
                fotoPictureBox.Image = Image.FromFile(ofd.FileName);
            }
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            if (!Utils.temCamposVazio(this))
            {
                UsuarioRepository repository = new UsuarioRepository();
                byte[] imgArray = null;
                
                using (MemoryStream ms = new MemoryStream())
                {
                    Image img = fotoPictureBox.Image;
                    img.Save(ms, ImageFormat.Png);
                    imgArray = ms.ToArray();
                }

                if (this.usuario == null)
                {
                    Usuario usuario = new Usuario
                    {
                        Nome = nomeTextBox.Text,
                        Email = emailTextBox.Text,
                        Senha = "1234",
                        Telefone = telefoneTextBox.Text,
                        Username = usuarioTextBox.Text,
                        Foto = imgArray
                    };

                    repository.adicionar(usuario);

                    //Todos os campos foram preenchidos - Salva os campos no BD
                    MessageBox.Show("Dados Salvos.",
                                    "Aviso", MessageBoxButtons.OK,
                                     MessageBoxIcon.Information);
                }
                else
                {
                    this.usuario.Nome = nomeTextBox.Text;
                    this.usuario.Email = emailTextBox.Text;
                    this.usuario.Senha = "1234";
                    this.usuario.Telefone = telefoneTextBox.Text;
                    this.usuario.Username = usuarioTextBox.Text;
                    this.usuario.Foto = imgArray;

                    repository.editar(usuario);
                }
                this.Close();

            }
            else
            {   //Texto , Título, Botões, Ícone
                MessageBox.Show("Todos os campos são obrigatórios.",
                                "Aviso", MessageBoxButtons.OK,
                                 MessageBoxIcon.Information);
            }
        }

        private void emailTextBox_TextChanged(object sender, EventArgs e)
        {
            //cria um padrão regex
            string pattern = "[a-z]{3,}[.][a-z]{3,}[@][a-z]{3,}[.][a-z]{2,3}";
            //seta uma propriedade com a verificação do regex
            isValidEmail = Regex.IsMatch(emailTextBox.Text, pattern);
            //Altera a visualização da label
            lblEmailValido.Visible = false;

        }

        private void TeclaPressioada(object sender, KeyEventArgs e)
        {

        }

        private void btnSalvarFoto_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "jpeg Image|*.jpeg|png Image|*.png";
            sfd.Title = "Salvar uma imagem";

            Image image = fotoPictureBox.Image;

            if (image != null)
            {
                if (sfd.ShowDialog() == DialogResult.OK && sfd.FileName.Trim().Length != 0)
                {
                    FileStream fs = sfd.OpenFile() as FileStream;

                    switch (sfd.FilterIndex)
                    {
                        case 1:
                            image.Save(fs, ImageFormat.Jpeg);
                            break;
                        case 2:
                            image.Save(fs, ImageFormat.Png);
                            break;
                        default:
                            break;
                    }
                }
            }
            else
                MessageBox.Show("Selecione uma imagem antes de salvar", "Erro");
        }
    }
}
