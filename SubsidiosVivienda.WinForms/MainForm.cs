using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SubsidiosVivienda.WinForms
{
    public class MainForm : Form
    {
        private const decimal SalarioMinimo = 1_160_000m;

        private readonly SubsidioDataService _dataService;
        private readonly CultureInfo _cultura = new("es-CO");
        private readonly BindingList<SubsidioAplicado> _aplicados = new();
        private readonly ToolTip _toolTip = new();

        private ComboBox _cmbEstrato = null!;
        private TextBox _txtCedula = null!;
        private TextBox _txtNombre = null!;
        private TextBox _txtIngreso = null!;
        private CheckBox _chkCasaPropia = null!;
        private TextBox _txtSubsidio = null!;
        private TextBox _txtSubsidioLetras = null!;
        private Button _btnCalcular = null!;
        private Button _btnGuardar = null!;
        private Button _btnCancelar = null!;
        private DataGridView _dgvDisponibles = null!;
        private DataGridView _dgvAplicados = null!;
        private DataGridView _dgvResumen = null!;
        private Label _lblTotalSubsidios = null!;
        private Label _lblTotalFamilias = null!;

        private decimal? _subsidioCalculado;

        public MainForm()
        {
            Text = "Sistema de Subsidios de Vivienda";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1100, 720);

            _dataService = new SubsidioDataService();

            InicializarComponentes();
            CargarDisponibilidades();
            CargarAplicados();
            ActualizarResumen();
        }

        private void InicializarComponentes()
        {
            var layoutPrincipal = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                AutoSize = true
            };
            layoutPrincipal.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layoutPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
            layoutPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
            layoutPrincipal.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var grpFormulario = new GroupBox
            {
                Text = "Datos de la familia",
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(10)
            };

            var formulario = new TableLayoutPanel
            {
                ColumnCount = 4,
                RowCount = 5,
                Dock = DockStyle.Fill,
                AutoSize = true
            };
            formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));

            formulario.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            formulario.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            formulario.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            formulario.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            formulario.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _txtCedula = CrearTextBox();
            _txtNombre = CrearTextBox();
            _cmbEstrato = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill
            };
            _cmbEstrato.Items.AddRange(new object[] { "1", "2", "3", "4" });
            _cmbEstrato.SelectedIndexChanged += (_, _) => MostrarTooltipEstrato();
            _cmbEstrato.MouseMove += (_, e) => MostrarTooltipEstrato();

            _txtIngreso = CrearTextBox();
            _chkCasaPropia = new CheckBox { Text = "¿Casa propia?", Dock = DockStyle.Left };
            _txtSubsidio = CrearTextBox();
            _txtSubsidio.ReadOnly = true;
            _txtSubsidio.BackColor = SystemColors.ControlLight;

            _txtSubsidioLetras = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                Height = 60,
                BackColor = SystemColors.ControlLight
            };

            _btnCalcular = new Button { Text = "Calcular", AutoSize = true };
            _btnCalcular.Click += (_, _) => CalcularSubsidio();
            _btnGuardar = new Button { Text = "Guardar", AutoSize = true };
            _btnGuardar.Click += (_, _) => GuardarSubsidio();
            _btnCancelar = new Button { Text = "Cancelar", AutoSize = true };
            _btnCancelar.Click += (_, _) => LimpiarFormulario();

            formulario.Controls.Add(new Label { Text = "Cédula", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            formulario.Controls.Add(_txtCedula, 1, 0);
            formulario.Controls.Add(new Label { Text = "Nombre completo", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 2, 0);
            formulario.Controls.Add(_txtNombre, 3, 0);

            formulario.Controls.Add(new Label { Text = "Estrato", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
            formulario.Controls.Add(_cmbEstrato, 1, 1);
            formulario.Controls.Add(new Label { Text = "Ingreso mensual", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 2, 1);
            formulario.Controls.Add(_txtIngreso, 3, 1);

            formulario.Controls.Add(new Label { Text = "Casa propia", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
            formulario.Controls.Add(_chkCasaPropia, 1, 2);
            formulario.Controls.Add(new Label { Text = "Subsidio asignado", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 2, 2);
            formulario.Controls.Add(_txtSubsidio, 3, 2);

            formulario.Controls.Add(new Label { Text = "Valor en letras", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
            formulario.SetColumnSpan(_txtSubsidioLetras, 3);
            formulario.Controls.Add(_txtSubsidioLetras, 1, 3);

            var panelBotones = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Fill,
                AutoSize = true
            };
            panelBotones.Controls.AddRange(new Control[] { _btnCalcular, _btnGuardar, _btnCancelar });
            formulario.Controls.Add(panelBotones, 1, 4);
            formulario.SetColumnSpan(panelBotones, 3);

            grpFormulario.Controls.Add(formulario);

            var grpDisponibles = new GroupBox
            {
                Text = "Disponibilidad de subsidios",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            _dgvDisponibles = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false
            };
            _dgvDisponibles.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Estrato",
                DataPropertyName = nameof(SubsidioDisponibilidad.Estrato),
                Width = 80
            });
            _dgvDisponibles.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Valor disponible",
                DataPropertyName = nameof(SubsidioDisponibilidad.ValorDisponible),
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C0", FormatProvider = _cultura },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            _dgvDisponibles.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Valor subsidio",
                DataPropertyName = nameof(SubsidioDisponibilidad.ValorSubsidio),
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C0", FormatProvider = _cultura },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            grpDisponibles.Controls.Add(_dgvDisponibles);

            var grpAplicados = new GroupBox
            {
                Text = "Subsidios asignados",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            _dgvAplicados = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false
            };
            _dgvAplicados.CellFormatting += DgvAplicadosOnCellFormatting;
            _dgvAplicados.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cédula", DataPropertyName = nameof(SubsidioAplicado.Cedula), Width = 120 });
            _dgvAplicados.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nombre", DataPropertyName = nameof(SubsidioAplicado.NombreCompleto), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _dgvAplicados.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Estrato", DataPropertyName = nameof(SubsidioAplicado.Estrato), Width = 80 });
            _dgvAplicados.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Ingreso mensual",
                DataPropertyName = nameof(SubsidioAplicado.IngresoMensual),
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C0", FormatProvider = _cultura },
                Width = 150
            });
            _dgvAplicados.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Casa propia", DataPropertyName = nameof(SubsidioAplicado.CasaPropia), Width = 110 });
            _dgvAplicados.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Subsidio asignado",
                DataPropertyName = nameof(SubsidioAplicado.SubsidioAsignado),
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C0", FormatProvider = _cultura },
                Width = 150
            });
            grpAplicados.Controls.Add(_dgvAplicados);

            var grpResumen = new GroupBox
            {
                Text = "Resumen",
                Dock = DockStyle.Bottom,
                Padding = new Padding(10),
                AutoSize = true
            };

            var resumenLayout = new TableLayoutPanel
            {
                ColumnCount = 2,
                Dock = DockStyle.Fill,
                AutoSize = true
            };
            resumenLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            resumenLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            _dgvResumen = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = false,
                RowHeadersVisible = false,
                Height = 150
            };
            _dgvResumen.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Detalle", DataPropertyName = "Etiqueta", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _dgvResumen.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Valor", DataPropertyName = "Valor", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });

            var totalesPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                Dock = DockStyle.Fill,
                AutoSize = true
            };
            _lblTotalSubsidios = new Label { AutoSize = true, Font = new Font(Font, FontStyle.Bold) };
            _lblTotalFamilias = new Label { AutoSize = true, Font = new Font(Font, FontStyle.Bold) };
            totalesPanel.Controls.Add(_lblTotalSubsidios);
            totalesPanel.Controls.Add(_lblTotalFamilias);

            resumenLayout.Controls.Add(_dgvResumen, 0, 0);
            resumenLayout.Controls.Add(totalesPanel, 1, 0);
            grpResumen.Controls.Add(resumenLayout);

            layoutPrincipal.Controls.Add(grpFormulario, 0, 0);
            layoutPrincipal.Controls.Add(grpDisponibles, 0, 1);
            layoutPrincipal.Controls.Add(grpAplicados, 0, 2);
            layoutPrincipal.Controls.Add(grpResumen, 0, 3);

            Controls.Add(layoutPrincipal);
        }

        private static TextBox CrearTextBox() => new() { Dock = DockStyle.Fill };

        private void CargarDisponibilidades()
        {
            var disponibilidades = _dataService.ObtenerDisponibilidades().OrderBy(d => d.Estrato).ToList();
            _dgvDisponibles.DataSource = disponibilidades;
        }

        private void CargarAplicados()
        {
            _aplicados.Clear();
            foreach (var aplicado in _dataService.LeerAplicados())
            {
                _aplicados.Add(aplicado);
            }

            _dgvAplicados.DataSource = _aplicados;
        }

        private void CalcularSubsidio()
        {
            if (!ValidarEntradasBasicas(out var estrato))
            {
                return;
            }

            if (_chkCasaPropia.Checked)
            {
                MessageBox.Show("No es posible asignar subsidio a familias con casa propia.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var disponible = _dataService.ObtenerValorDisponible(estrato);
            if (disponible <= 0)
            {
                MessageBox.Show("No hay subsidios disponibles para el estrato seleccionado.", "Sin cupo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var valorSubsidio = _dataService.ObtenerSubsidio(estrato);
            _subsidioCalculado = valorSubsidio;
            _txtSubsidio.Text = valorSubsidio.ToString("C0", _cultura);
            _txtSubsidioLetras.Text = NumeroALetrasConverter.Convertir(valorSubsidio);
        }

        private void GuardarSubsidio()
        {
            if (!ValidarEntradasBasicas(out var estrato))
            {
                return;
            }

            if (_chkCasaPropia.Checked)
            {
                MessageBox.Show("No es posible asignar subsidio a familias con casa propia.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_subsidioCalculado == null)
            {
                MessageBox.Show("Debe calcular el subsidio antes de guardar.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var disponible = _dataService.ObtenerValorDisponible(estrato);
            if (disponible < _subsidioCalculado)
            {
                MessageBox.Show("El valor disponible es inferior al subsidio a asignar.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var aplicado = new SubsidioAplicado
            {
                Cedula = _txtCedula.Text.Trim(),
                NombreCompleto = _txtNombre.Text.Trim(),
                Estrato = estrato,
                IngresoMensual = decimal.Parse(_txtIngreso.Text.Trim(), NumberStyles.Number, _cultura),
                CasaPropia = _chkCasaPropia.Checked,
                SubsidioAsignado = _subsidioCalculado.Value
            };

            _dataService.GuardarAplicacion(aplicado);
            _dataService.DescontarSubsidio(estrato);

            _aplicados.Add(aplicado);
            CargarDisponibilidades();
            ActualizarResumen();
            LimpiarFormulario();

            MessageBox.Show("Subsidio asignado correctamente.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LimpiarFormulario()
        {
            _txtCedula.Clear();
            _txtNombre.Clear();
            _cmbEstrato.SelectedIndex = -1;
            _txtIngreso.Clear();
            _chkCasaPropia.Checked = false;
            _txtSubsidio.Clear();
            _txtSubsidioLetras.Clear();
            _subsidioCalculado = null;
            _txtCedula.Focus();
        }

        private bool ValidarEntradasBasicas(out int estrato)
        {
            estrato = 0;
            if (string.IsNullOrWhiteSpace(_txtCedula.Text) || !Regex.IsMatch(_txtCedula.Text.Trim(), "^\\d{5,}$"))
            {
                MessageBox.Show("Ingrese una cédula válida (solo números).", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtCedula.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(_txtNombre.Text) || !Regex.IsMatch(_txtNombre.Text.Trim(), "^[A-Za-zÁÉÍÓÚáéíóúÑñ ]+$"))
            {
                MessageBox.Show("Ingrese un nombre válido (solo letras y espacios).", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtNombre.Focus();
                return false;
            }

            if (_cmbEstrato.SelectedIndex < 0 || !int.TryParse(_cmbEstrato.SelectedItem.ToString(), out estrato))
            {
                MessageBox.Show("Seleccione un estrato entre 1 y 4.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _cmbEstrato.DroppedDown = true;
                return false;
            }

            if (!decimal.TryParse(_txtIngreso.Text.Trim(), NumberStyles.Number, _cultura, out var ingreso) || ingreso <= 0)
            {
                MessageBox.Show("Ingrese un valor numérico válido para el ingreso mensual.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtIngreso.Focus();
                return false;
            }

            if (ingreso > SalarioMinimo * 4)
            {
                MessageBox.Show("El ingreso mensual no puede superar 4 salarios mínimos.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtIngreso.Focus();
                return false;
            }

            return true;
        }

        private void MostrarTooltipEstrato()
        {
            if (_cmbEstrato.SelectedIndex >= 0 && int.TryParse(_cmbEstrato.SelectedItem.ToString(), out int estrato))
            {
                var disponible = _dataService.ObtenerValorDisponible(estrato);
                _toolTip.SetToolTip(_cmbEstrato, $"Disponible: {disponible.ToString("C0", _cultura)}");
            }
        }

        private void ActualizarResumen()
        {
            var totalSubsidios = _aplicados.Sum(a => a.SubsidioAsignado);
            var totalFamilias = _aplicados.Count;

            var datos = new List<ResumenItem>
            {
                new() { Etiqueta = "Total subsidios asignados", Valor = totalSubsidios.ToString("C0", _cultura) },
                new() { Etiqueta = "Total familias beneficiadas", Valor = totalFamilias.ToString() }
            };

            for (int estrato = 1; estrato <= 4; estrato++)
            {
                var sumatoria = _aplicados.Where(a => a.Estrato == estrato).Sum(a => a.SubsidioAsignado);
                datos.Add(new ResumenItem
                {
                    Etiqueta = $"Sumatoria estrato {estrato}",
                    Valor = sumatoria.ToString("C0", _cultura)
                });
            }

            _dgvResumen.DataSource = datos;
            _lblTotalSubsidios.Text = $"Total subsidios: {totalSubsidios.ToString("C0", _cultura)}";
            _lblTotalFamilias.Text = $"Familias beneficiadas: {totalFamilias}";
        }

        private void DgvAplicadosOnCellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_dgvAplicados.Columns[e.ColumnIndex].DataPropertyName == nameof(SubsidioAplicado.CasaPropia) && e.Value is bool valor)
            {
                e.Value = valor ? "SI" : "NO";
                e.FormattingApplied = true;
            }
        }
    }
}
