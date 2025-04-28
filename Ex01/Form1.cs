using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Ex01
{
    public partial class Form_telaprincial : Form
    {
        MySqlConnection Conexão;

        private string data_source = "server=localhost;user=root;password=Andre*1995;database=aulas";

        public int? id_contato_selecionado = 0;

        public void zerar_forms()
        {
            id_contato_selecionado = null;
            txt_nome.Text = string.Empty;
            txt_email.Text = "";
            txt_telefone.Text = "";
            txt_nome.Focus();
        }

        public Form_telaprincial()
        {
            InitializeComponent();

            lst_contato.MouseClick += lst_contato_MouseClick;

            // Configurações do ListView
            lst_contato.View = View.Details;
            lst_contato.LabelEdit = true;
            lst_contato.AllowColumnReorder = true;
            lst_contato.FullRowSelect = true;
            lst_contato.GridLines = true;

            // Cabeçalhos
            lst_contato.Columns.Add("ID", 30, HorizontalAlignment.Left);
            lst_contato.Columns.Add("Nome", 150, HorizontalAlignment.Left);
            lst_contato.Columns.Add("Email", 150, HorizontalAlignment.Left);
            lst_contato.Columns.Add("Telefone", 150, HorizontalAlignment.Left);
        }

        private void btn_salvar_Click(object sender, EventArgs e)
        {
            try
            {
                using (var conexao = new MySqlConnection(data_source))
                {
                    conexao.Open();

                    if (id_contato_selecionado == null) // CORRIGIDO: se for novo
                    {
                        string sqlInsert = "INSERT INTO contato (nome, email, telefone) VALUES (@nome, @email, @telefone)";
                        using (var insert = new MySqlCommand(sqlInsert, conexao))
                        {
                            insert.Parameters.AddWithValue("@nome", txt_nome.Text);
                            insert.Parameters.AddWithValue("@email", txt_email.Text);
                            insert.Parameters.AddWithValue("@telefone", txt_telefone.Text);
                            insert.ExecuteNonQuery();
                        }
                        MessageBox.Show("Contato cadastrado com sucesso!");
                    }
                    else // Se for existente (editar)
                    {
                        string sqlUpdate = "UPDATE contato SET nome = @nome, email = @email, telefone = @telefone WHERE id = @id";
                        using (var update = new MySqlCommand(sqlUpdate, conexao))
                        {
                            update.Parameters.AddWithValue("@nome", txt_nome.Text);
                            update.Parameters.AddWithValue("@email", txt_email.Text);
                            update.Parameters.AddWithValue("@telefone", txt_telefone.Text);
                            update.Parameters.AddWithValue("@id", id_contato_selecionado);
                            update.ExecuteNonQuery();
                        }
                        MessageBox.Show("Contato atualizado com sucesso!");
                    }
                }

                zerar_forms();
                btn_buscar_Click(null, null); // Atualiza a lista
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
        }

        private void btn_buscar_Click(object sender, EventArgs e)
        {
            try
            {
                string q = "%" + txt_buscar.Text + "%";
                Conexão = new MySqlConnection(data_source);

                string sql = "SELECT * FROM contato WHERE nome LIKE @q OR email LIKE @q";
                Conexão.Open();

                MySqlCommand buscar = new MySqlCommand(sql, Conexão);
                buscar.Parameters.AddWithValue("@q", q);

                MySqlDataReader reader = buscar.ExecuteReader();

                lst_contato.Items.Clear();

                while (reader.Read())
                {
                    string[] row =
                    {
                        reader.GetInt32(0).ToString(), // id
                        reader.GetString(1),           // nome
                        reader.GetString(2),           // email
                        reader.GetString(3),           // telefone
                    };

                    var linha_list_view = new ListViewItem(row);
                    lst_contato.Items.Add(linha_list_view);
                }

                id_contato_selecionado = null; // CORRIGIDO: limpa a seleção
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
            finally
            {
                Conexão.Close();
            }
        }

        private void txt_nome_TextChanged(object sender, EventArgs e)
        {
            // Deixe vazio ou remova se não usar
        }

        private void lst_contato_SelectedIndexChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            ListView.SelectedListViewItemCollection itens_selecionados = lst_contato.SelectedItems;

            foreach (ListViewItem item in itens_selecionados)
            {
                id_contato_selecionado = Convert.ToInt32(item.SubItems[0].Text);
                txt_nome.Text = item.SubItems[1].Text;
                txt_email.Text = item.SubItems[2].Text;
                txt_telefone.Text = item.SubItems[3].Text;
            }
        }

        private void btn_novo_Click(object sender, EventArgs e)
        {
            zerar_forms();
        }

        private void excluirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (id_contato_selecionado > 0)
            {
                DialogResult conf = MessageBox.Show("Deseja excluir este registro?", "Confirmação", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (conf == DialogResult.Yes)
                {
                    try
                    {
                        using (var conexao = new MySqlConnection(data_source))
                        {
                            conexao.Open();
                            var cmd = new MySqlCommand("DELETE FROM contato WHERE id = @id", conexao);
                            cmd.Parameters.AddWithValue("@id", id_contato_selecionado);
                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("Contato excluído com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        zerar_forms();
                        btn_buscar_Click(null, null); // Atualiza lista
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erro ao excluir: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Selecione um contato para excluir!", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void lst_contato_MouseClick(object sender, MouseEventArgs e)
        {
            {
                if (lst_contato.SelectedItems.Count > 0)
                {
                    ListViewItem item = lst_contato.SelectedItems[0];
                    id_contato_selecionado = Convert.ToInt32(item.SubItems[0].Text);
                }
            }
        }
    }
}
