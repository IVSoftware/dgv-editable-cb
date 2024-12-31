
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization.DataContracts;
using System.Windows.Forms;

namespace dgv_editable_cb
{
    public partial class MainForm : Form
    {
        public MainForm() => InitializeComponent();
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            dataGridView.DataSource = Records;
            dataGridView.Columns[nameof(Record.Description)].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            var indexReplace = dataGridView.Columns[nameof(Record.Selectable)].Index;

            // Swap out the default bound column for the custom one.
            var dropListColumn = new DataGridViewComboBoxColumnEx
            { 
                Name = "DropList", 
                DataPropertyName = nameof(Record.Selectable),
            };
            dropListColumn.Items.AddRange(["Option A", "Option B", "Option C"]);
            dataGridView.Columns.RemoveAt(indexReplace);
            dataGridView.Columns.Insert(indexReplace, dropListColumn);

            dataGridView.CellValidating += (sender, e) =>
            {
                Debug.WriteLine($"Validating {e.FormattedValue}");
            };
            dataGridView.CellValidated += (sender, e) =>
            {
                if (Records[e.RowIndex] is not null)
                {
                    Debug.WriteLine($"Validated {Records[e.RowIndex].Selectable}");
                }
            };
            for (int i = 1; i <= 5; i++)
            {
                Records.Add(new Record
                {
                    Description = $"Item {i}"
                });
            }
        }
        BindingList<Record> Records = new BindingList<Record>();        
    }
    class Record
    {
        public string? Description { get; set; }
        public string? Selectable { get; set; }
    }
    class DataGridViewComboBoxColumnEx : DataGridViewComboBoxColumn, IEnumerable
    {
        public DataGridViewComboBoxColumnEx() => CellTemplate = new DataGridViewComboBoxCellEx();
        public IEnumerator GetEnumerator() => Items.GetEnumerator();
        public void Add(object value) => Items.Add(value);
    }
    class DataGridViewComboBoxCellEx : DataGridViewComboBoxCell
    {        
        public override Type EditType => typeof(DataGridViewComboBoxEditingControlEx);
        protected override object GetFormattedValue(
            object value,
            int rowIndex,
            ref DataGridViewCellStyle cellStyle, 
            TypeConverter valueTypeConverter,
            TypeConverter formattedValueTypeConverter,
            DataGridViewDataErrorContexts context)
        {
            return value?.ToString() ?? string.Empty;
        }
        public override void InitializeEditingControl(
            int rowIndex,
            object initialFormattedValue, 
            DataGridViewCellStyle dataGridViewCellStyle)
        {
            base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
            if(DataGridView?.EditingControl is DataGridViewComboBoxEditingControlEx comboBoxEx)
            {
                comboBoxEx.DropDownStyle = ComboBoxStyle.DropDown;
                comboBoxEx.Text = initialFormattedValue?.ToString() ?? string.Empty ;
            }
        }
    }

    class DataGridViewComboBoxEditingControlEx : DataGridViewComboBoxEditingControl
    {
        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            DroppedDown = false;
            switch (e.KeyData)
            {
                case Keys.Escape:
                    _key = Keys.None;
                    BeginInvoke(() =>
                    {
                        EditingControlDataGridView?.CancelEdit();
                    });
                    break;
                case Keys.Return:
                    _key = Keys.None;
                    BeginInvoke(() =>
                    {
                        EditingControlDataGridView?.CommitEdit(DataGridViewDataErrorContexts.Commit);
                    });
                    break;
                default:
                    _key = e.KeyData;
                    break;
            }
        }
        Keys _key = Keys.None;

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (_key == Keys.None)
            {   /* G T K */
                // Text has changed programmatically
            }
            else
            {
                EditingControlDataGridView?.NotifyCurrentCellDirty(true);
            }
        }
        protected override void OnSelectionChangeCommitted(EventArgs e)
        {
            base.OnSelectionChangeCommitted(e);
            BeginInvoke(() =>
            {
                EditingControlDataGridView?.CommitEdit(DataGridViewDataErrorContexts.Commit);
            });
        }
    }
}
