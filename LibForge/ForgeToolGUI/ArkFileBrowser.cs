﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibForge.Mesh;
using LibForge.Midi;
using LibForge.SongData;
using LibForge.Texture;

namespace ForgeToolGUI
{
  public partial class ArkFileBrowser : Form
  {
    public ArkFileBrowserState state;
    public ArkFileBrowser()
    {
      state = new ArkFileBrowserState();
      InitializeComponent();
    }

    private void LoadArk(string filename)
    {
      if (state.Loaded) Unload();
      state.ark = GameArchives.Ark.ArkPackage.OpenFile(GameArchives.Util.LocalFile(filename));
      state.root = state.ark.RootDirectory;
      FinishLoad();
    }

    private void LoadFolder(string path)
    {
      if (state.Loaded) Unload();
      state.root = GameArchives.Util.LocalDir(path);
      FinishLoad();
    }

    private void FinishLoad()
    {
      closeToolStripMenuItem.Enabled = true;
      void AddNodes(GameArchives.IDirectory dir, TreeNodeCollection nodes)
      {
        foreach (var d in dir.Dirs)
        {
          var node = new TreeNode(d.Name);
          nodes.Add(node);
          AddNodes(d, node.Nodes);
        }
        foreach (var f in dir.Files)
        {
          var node = new TreeNode(f.Name);
          node.Tag = f;
          nodes.Add(node);
        }
      }
      AddNodes(state.root, fileTreeView.Nodes);
      state.Loaded = true;
    }

    private void Unload()
    {
      if (!state.Loaded) return;
      fileTreeView.Nodes.Clear();
      state.root = null;
      if (state.ark != null)
      {
        state.ark.Dispose();
        state.ark = null;
      }
      closeToolStripMenuItem.Enabled = false;
      state.Loaded = false;
    }

    private void openToolStripMenuItem_Click(object sender, EventArgs e)
    {
      OpenFileDialog of = new OpenFileDialog();
      of.Filter = "Ark Header (*.hdr)|*.hdr";
      if(of.ShowDialog(this) == DialogResult.OK)
      {
        LoadArk(of.FileName);
      }
    }

    private void closeToolStripMenuItem_Click(object sender, EventArgs e)
    {
      Unload();
    }

    private void exitToolStripMenuItem_Click(object sender, EventArgs e)
    {
      Application.Exit();
    }

    private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
    {
      switch (e.Node.Tag)
      {
        case null:
          break;
        case GameArchives.IFile i:
          if (i.Name.Contains(".bmp_") || i.Name.Contains(".png_"))
          {
            using (var s = i.GetStream())
            {
              var tex = TextureReader.ReadStream(s);
              pictureBox1.Width = tex.Mipmaps[0].Width;
              pictureBox1.Height = tex.Mipmaps[0].Height;
              tabControl1.SelectTab(0);
              fileTreeView.Select();
              try
              {
                pictureBox1.Image = TextureConverter.ToBitmap(tex, 0);
              }
              catch(Exception ex)
              {
                MessageBox.Show("Couldn't load texture: " + ex.Message);
              }
            }
          }
          else if(i.Name.Contains("_dta_"))
          {
            using (var s = i.GetStream())
            {
              var data = DtxCS.DTX.FromDtb(s);
              tabControl1.SelectTab(1);
              fileTreeView.Select();
              var sb = new StringBuilder();
              foreach (var x in data.Children)
              {
                sb.AppendLine(x.ToString(0));
              }
              dataTextBox.Text = sb.ToString();
            }
          }
          else if(i.Name.EndsWith(".dta") || i.Name.EndsWith(".moggsong"))
          {
            using (var s = i.GetStream())
            using (var r = new System.IO.StreamReader(s))
            {
              tabControl1.SelectTab(1);
              fileTreeView.Select();
              dataTextBox.Text = r.ReadToEnd();
            }
          }
          else if(i.Name.Contains(".songdta"))
          {
            using (var s = i.GetStream())
            {
              var songData = SongDataReader.ReadStream(s);
              songDataInspector1.UpdateValues(songData);
              tabControl1.SelectTab(2);
              fileTreeView.Select();
            }
          }
          else if(i.Name.Contains(".fbx"))
          {
            using (var s = i.GetStream())
            {
              var mesh = HxMeshReader.ReadStream(s);
              meshTextBox.Text = HxMeshConverter.ToObj(mesh);
              tabControl1.SelectTab(3);
              fileTreeView.Select();
            }
          }
          else if(i.Name.Contains(".rbmid_"))
          {
            using (var s = i.GetStream())
            {
              var rbmid = RBMidReader.ReadStream(s);
              treeView1.Nodes.Clear();
              AddObjectNodes(rbmid, treeView1.Nodes);
              tabControl1.SelectTab(4);
              fileTreeView.Select();
            }
          }
          break;
      }
    }

    void AddObjectNodes(object obj, TreeNodeCollection nodes)
    {
      if (obj == null) return;
      var fields = obj.GetType().GetFields();
      foreach(var f in fields)
      {
        if (f.IsLiteral) continue;
        if (f.FieldType.IsPrimitive || f.FieldType == typeof(string))
        {
          nodes.Add(f.Name + " = " + f.GetValue(obj).ToString());
        }
        else if(f.FieldType.IsArray)
        {
          AddArrayNodes(f.GetValue(obj) as Array, f.Name, nodes);
        }
        else
        {
          var node = new TreeNode(f.Name);
          AddObjectNodes(f.GetValue(obj), node.Nodes);
          nodes.Add(node);
        }
      }
    }

    void AddArrayNodes(Array arr, string name, TreeNodeCollection nodes)
    {
      var node = new TreeNode($"{name} ({arr.Length})");
      var eType = arr.GetType().GetElementType();
      if (eType.IsPrimitive || eType == typeof(string))
        for (var i = 0; i < arr.Length; i++)
        {
          var n = new TreeNode($"{name}[{i}] = {arr.GetValue(i)}");
          node.Nodes.Add(n);
        }
      else for (var i = 0; i < arr.Length; i++)
        {
          var myName = $"{name}[{i}]";
          var n = new TreeNode(myName);
          node.Nodes.Add(n);
          if (eType.IsArray)
            AddArrayNodes(arr.GetValue(i) as Array, myName, n.Nodes);
          else
            AddObjectNodes(arr.GetValue(i), n.Nodes);
        }
      nodes.Add(node);
    }

    private void toolStripMenuItem1_Click(object sender, EventArgs e)
    {
      // Sorry.
      var of = new FolderBrowserDialog();
      of.ShowNewFolderButton = false;
      if(of.ShowDialog(this) == DialogResult.OK)
      {
        LoadFolder(of.SelectedPath);
      }
    }
  }
  public class ArkFileBrowserState
  {
    public bool Loaded = false;
    public GameArchives.Ark.ArkPackage ark;
    public GameArchives.IDirectory root;
  }
}
