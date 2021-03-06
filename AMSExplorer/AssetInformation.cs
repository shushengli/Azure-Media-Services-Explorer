﻿//----------------------------------------------------------------------- 
// <copyright file="AssetInformation.cs" company="Microsoft">Copyright (c) Microsoft Corporation. All rights reserved.</copyright> 
// <license>
// Azure Media Services Explorer Ver. 3.2
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
//  
// http://www.apache.org/licenses/LICENSE-2.0 
//  
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License. 
// </license> 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.WindowsAzure.MediaServices.Client;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;
using System.Web;
using Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization;
using Microsoft.WindowsAzure.MediaServices.Client.DynamicEncryption;
using Microsoft.WindowsAzure.MediaServices.Client.Metadata;

namespace AMSExplorer
{
    public partial class AssetInformation : Form
    {
        public IAsset myAsset;
        private string myAssetType;
        public CloudMediaContext myContext;
        public MediaServiceContextForDynManifest _contextdynmanifest;
        public IEnumerable<IStreamingEndpoint> myStreamingEndpoints;
        private ILocator tempLocator = null;
        private ILocator tempMetadaLocator = null;
        private IContentKeyAuthorizationPolicy myAuthPolicy = null;
        private Mainform myMainForm;
        private List<Filter> globalFilters;
        private bool oktobuildlocator = false;
        private ManifestTimingData myassetmanifesttimingdata = null;

        public AssetInformation(Mainform mainform)
        {
            InitializeComponent();
            this.Icon = Bitmaps.Azure_Explorer_ico;
            myMainForm = mainform;
        }

        private void contextMenuStripDG_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void toolStripMenuItemCopy_Click(object sender, EventArgs e)
        {
            if (TreeViewLocators.SelectedNode != null)
            {
                // Root node's Parent property is null, so do check
                if (TreeViewLocators.SelectedNode.Parent != null)
                {
                    System.Windows.Forms.Clipboard.SetText(TreeViewLocators.SelectedNode.Text);
                }

            }
        }

        private void toolStripMenuItemPlaybackFlash_Click(object sender, EventArgs e)
        {
            DoFlashPlayer();
        }

        private void DoFlashPlayer()
        {
            if (TreeViewLocators.SelectedNode != null)
            {
                // Root node's Parent property is null, so do check
                if (TreeViewLocators.SelectedNode.Parent != null)
                    switch (TreeViewLocators.SelectedNode.Parent.Text)
                    {
                        case AssetInfo._smooth_legacy:
                        case AssetInfo._smooth:
                            AssetInfo.DoPlayBackWithBestStreamingEndpoint(typeplayer: PlayerType.FlashAzurePage, Urlstr: TreeViewLocators.SelectedNode.Text, DoNotRewriteURL: true, context: myContext);
                            break;

                        case AssetInfo._dash:
                            AssetInfo.DoPlayBackWithBestStreamingEndpoint(typeplayer: PlayerType.DASHAzurePage, Urlstr: TreeViewLocators.SelectedNode.Text, DoNotRewriteURL: true, context: myContext);
                            break;

                        default:
                            break;
                    }

            }
        }

        private void toolStripMenuItemPlaybackSilverlight_Click(object sender, EventArgs e)
        {
            DoSLPlayer();

        }

        private void DoSLPlayer()
        {
            if (TreeViewLocators.SelectedNode != null)
            {
                // Root node's Parent property is null, so do check
                if (TreeViewLocators.SelectedNode.Parent != null)
                {
                    switch (TreeViewLocators.SelectedNode.Parent.Text)
                    {
                        case AssetInfo._smooth_legacy:
                        case AssetInfo._smooth:
                            AssetInfo.DoPlayBackWithBestStreamingEndpoint(typeplayer: PlayerType.SilverlightMonitoring, Urlstr: TreeViewLocators.SelectedNode.Text, DoNotRewriteURL: true, context: myContext);
                            break;

                        default:
                            break;
                    }

                }
                else
                {

                }
            }
        }

        private void toolStripMenuItemDASHIF_Click(object sender, EventArgs e)
        {
            DoDASHIFPlayer();

        }

        private void DoDASHIFPlayer()
        {
            if (TreeViewLocators.SelectedNode != null)
            {
                // Root node's Parent property is null, so do check
                if (TreeViewLocators.SelectedNode.Parent != null)
                {
                    switch (TreeViewLocators.SelectedNode.Parent.Text)
                    {
                        case AssetInfo._dash:
                            AssetInfo.DoPlayBackWithBestStreamingEndpoint(typeplayer: PlayerType.DASHIFRefPlayer, Urlstr: TreeViewLocators.SelectedNode.Text, DoNotRewriteURL: true, context: myContext);
                            break;

                        default:
                            break;
                    }

                }

            }
        }

        private void contextMenuStripLocators_Opening(object sender, CancelEventArgs e)
        {
            if (TreeViewLocators.SelectedNode != null)
            {
                if (TreeViewLocators.SelectedNode.Parent != null)
                {
                    toolStripMenuItemAzureMediaPlayer.Enabled = false;
                    toolStripMenuItemDASHIF.Enabled = false;
                    toolStripMenuItemDASHLiveAzure.Enabled = false;
                    toolStripMenuItemPlaybackFlashAzure.Enabled = false;
                    toolStripMenuItemPlaybackSilverlightMonitoring.Enabled = false;
                    toolStripMenuItemPlaybackMP4.Enabled = false;
                    toolStripMenuItemOpen.Enabled = false;
                    deleteLocatorToolStripMenuItem.Enabled = false;

                    if (TreeViewLocators.SelectedNode.Parent.Text.Equals(AssetInfo._smooth) | TreeViewLocators.SelectedNode.Parent.Text.Contains(AssetInfo._smooth_legacy))
                    {
                        toolStripMenuItemAzureMediaPlayer.Enabled = true;
                        toolStripMenuItemDASHLiveAzure.Enabled = false;
                        toolStripMenuItemDASHIF.Enabled = false;
                        toolStripMenuItemPlaybackFlashAzure.Enabled = true;
                        toolStripMenuItemPlaybackSilverlightMonitoring.Enabled = true;
                        toolStripMenuItemPlaybackMP4.Enabled = false;
                        toolStripMenuItemOpen.Enabled = false;
                    }
                    if (TreeViewLocators.SelectedNode.Parent.Text.Equals(AssetInfo._dash))
                    {
                        toolStripMenuItemAzureMediaPlayer.Enabled = true;
                        toolStripMenuItemDASHLiveAzure.Enabled = true;
                        toolStripMenuItemDASHIF.Enabled = true;
                        toolStripMenuItemPlaybackFlashAzure.Enabled = true;
                        toolStripMenuItemPlaybackSilverlightMonitoring.Enabled = false;
                        toolStripMenuItemPlaybackMP4.Enabled = false;
                        toolStripMenuItemOpen.Enabled = false;
                    }
                    if (TreeViewLocators.SelectedNode.Parent.Text.Equals(AssetInfo._prog_down_https_SAS))
                    {
                        toolStripMenuItemAzureMediaPlayer.Enabled = (TreeViewLocators.SelectedNode.Text.ToLower().Contains(".mp4"));
                        toolStripMenuItemDASHLiveAzure.Enabled = false;
                        toolStripMenuItemDASHIF.Enabled = false;
                        toolStripMenuItemPlaybackFlashAzure.Enabled = false;
                        toolStripMenuItemPlaybackSilverlightMonitoring.Enabled = false;
                        toolStripMenuItemPlaybackMP4.Enabled = false;
                        toolStripMenuItemOpen.Enabled = true;
                    }
                    if (TreeViewLocators.SelectedNode.Parent.Text.Equals(AssetInfo._prog_down_http_streaming))
                    {
                        toolStripMenuItemAzureMediaPlayer.Enabled = (TreeViewLocators.SelectedNode.Text.ToLower().Contains(".mp4"));
                        toolStripMenuItemDASHLiveAzure.Enabled = false;
                        toolStripMenuItemDASHIF.Enabled = false;
                        toolStripMenuItemPlaybackFlashAzure.Enabled = false;
                        toolStripMenuItemPlaybackSilverlightMonitoring.Enabled = false;
                        toolStripMenuItemPlaybackMP4.Enabled = (TreeViewLocators.SelectedNode.Text.ToLower().Contains(".mp4"));
                        toolStripMenuItemOpen.Enabled = !(TreeViewLocators.SelectedNode.Text.ToLower().Contains(".ism"));
                    }
                }
                else
                {
                    deleteLocatorToolStripMenuItem.Enabled = true; // no parent, so we can propose the deletion
                }

            }
        }

        private void toolStripMenuItemPlaybackMP4_Click(object sender, EventArgs e)
        {
            DoHTMLPlayer();
        }



        private void DoHTMLPlayer()
        {
            if (TreeViewLocators.SelectedNode != null)
            {
                // Root node's Parent property is null, so do check
                if (TreeViewLocators.SelectedNode.Parent != null)
                {
                    AssetInfo.DoPlayBackWithBestStreamingEndpoint(typeplayer: PlayerType.MP4AzurePage, Urlstr: TreeViewLocators.SelectedNode.Text, DoNotRewriteURL: true, context: myContext);
                }
            }
        }

        private void toolStripMenuItemOpen_Click(object sender, EventArgs e)
        {
            if (TreeViewLocators.SelectedNode != null)
            {
                // Root node's Parent property is null, so do check
                if (TreeViewLocators.SelectedNode.Parent != null)
                {
                    Process.Start(TreeViewLocators.SelectedNode.Text);
                }
            }
        }


        private long ListAssetFiles()
        {
            long size = 0;
            bool bfileinasset = (myAsset.AssetFiles.Count() == 0) ? false : true;
            listViewFiles.Items.Clear();
            DGFiles.Rows.Clear();
            if (bfileinasset)
            {
                listViewFiles.BeginUpdate();
                foreach (IAssetFile file in myAsset.AssetFiles)
                {
                    ListViewItem item = new ListViewItem(file.Name, 0);
                    if (file.IsPrimary) item.ForeColor = Color.Blue;
                    item.SubItems.Add(AssetInfo.FormatByteSize(file.ContentFileSize));
                    listViewFiles.Items.Add(item);
                    size += file.ContentFileSize;
                }
                listViewFiles.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                listViewFiles.EndUpdate();
            }
            return size;
        }

        private long ListAssetKeys()
        {
            long size = 0;
            bool bkeyinasset = (myAsset.ContentKeys.Count() == 0) ? false : true;
            listViewKeys.Items.Clear();
            dataGridViewKeys.Rows.Clear();
            listViewAutPolOptions.Items.Clear();
            dataGridViewAutPolOption.Rows.Clear();
            buttonRemoveKey.Enabled = false;

            if (bkeyinasset)
            {
                listViewKeys.BeginUpdate();
                foreach (IContentKey key in myAsset.ContentKeys)
                {
                    ListViewItem item;
                    if (key.Name != null)
                    {
                        item = new ListViewItem(key.Name, 0);
                    }
                    else
                    {
                        item = new ListViewItem("<no name>", 0);
                    }
                    listViewKeys.Items.Add(item);
                }
                listViewKeys.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                listViewKeys.EndUpdate();
            }
            return size;
        }

        private void ListAssetDeliveryPolicies()
        {
            listViewDelPol.Items.Clear();
            buttonRemoveDelPol.Enabled = false;

            DGDelPol.Rows.Clear();
            listViewDelPol.BeginUpdate();
            foreach (var DelPol in myAsset.DeliveryPolicies)
            {
                ListViewItem item = new ListViewItem(DelPol.Name, 0);
                listViewDelPol.Items.Add(item);
            }
            listViewDelPol.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listViewDelPol.EndUpdate();
        }

        private void AssetInformation_Load(object sender, EventArgs e)
        {
            labelAssetNameTitle.Text += myAsset.Name;
            buttonSetPrimary.ForeColor = Color.Blue;

            myAssetType = AssetInfo.GetAssetType(myAsset);

            DGAsset.ColumnCount = 2;
            DGFiles.ColumnCount = 2;
            DGFiles.Columns[0].DefaultCellStyle.BackColor = Color.Gainsboro;
            dataGridViewAutPolOption.ColumnCount = 2;
            dataGridViewAutPolOption.Columns[0].DefaultCellStyle.BackColor = Color.Gainsboro;
            DGDelPol.ColumnCount = 2;
            DGDelPol.Columns[0].DefaultCellStyle.BackColor = Color.Gainsboro;
            dataGridViewKeys.ColumnCount = 2;
            dataGridViewKeys.Columns[0].DefaultCellStyle.BackColor = Color.Gainsboro;

            // Files in asset: headers
            long size = -1;
            if (myAsset.State != AssetState.Deleted)
            {
                size = ListAssetFiles();
                ListAssetDeliveryPolicies();
                ListAssetKeys();
            }

            // asset info
            DGAsset.Columns[0].DefaultCellStyle.BackColor = Color.Gainsboro;
            DGAsset.Rows.Add("Name", myAsset.Name);
            DGAsset.Rows.Add("Type", myAssetType);
            DGAsset.Rows.Add("AssetType", myAsset.AssetType);
            DGAsset.Rows.Add("Id", myAsset.Id);
            DGAsset.Rows.Add("AlternateId", myAsset.AlternateId);
            if (size != -1) DGAsset.Rows.Add("Size", AssetInfo.FormatByteSize(size));
            DGAsset.Rows.Add("State", (AssetState)myAsset.State);
            DGAsset.Rows.Add("Created", ((DateTime)myAsset.Created).ToLocalTime());
            DGAsset.Rows.Add("Last Modified", ((DateTime)myAsset.LastModified).ToLocalTime());
            DGAsset.Rows.Add("Creation Options", (AssetCreationOptions)myAsset.Options);

            var program = myContext.Programs.Where(p => p.AssetId == myAsset.Id).FirstOrDefault();
            if (program != null) // Asset is linked to a Program
            {
                DGAsset.Rows.Add("Program Id", program.Id);
            }


            if (myAsset.State != AssetState.Deleted)
            {
                DGAsset.Rows.Add("IsStreamable", myAsset.IsStreamable);
                DGAsset.Rows.Add("SupportsDynamicEncryption", myAsset.SupportsDynamicEncryption);
                DGAsset.Rows.Add("Uri", myAsset.Uri);
                DGAsset.Rows.Add("Storage Account Name", myAsset.StorageAccount.Name);
                DGAsset.Rows.Add("Storage Account Byte used", AssetInfo.FormatByteSize(myAsset.StorageAccount.BytesUsed));
                DGAsset.Rows.Add("Storage Account Is Default", myAsset.StorageAccount.IsDefault);

                foreach (IAsset p_asset in myAsset.ParentAssets)
                {
                    DGAsset.Rows.Add("Parent asset", p_asset.Name);
                    DGAsset.Rows.Add("Parent asset Id", p_asset.Id);
                }

                int i;
                IStreamingEndpoint SESelected = AssetInfo.GetBestStreamingEndpoint(myContext);

                foreach (var se in myStreamingEndpoints)
                {
                    i = comboBoxStreamingEndpoint.Items.Add(new Item(string.Format("{0} ({1}, {2} scale unit{3})", se.Name, se.State, se.ScaleUnits, se.ScaleUnits > 0 ? "s" : string.Empty), se.HostName));
                    if (se.Name == SESelected.Name) comboBoxStreamingEndpoint.SelectedIndex = comboBoxStreamingEndpoint.Items.Count - 1;
                }
                //BuildLocatorsTree(); anywy, it will be built when set the index for the filter drop list
                buttonUpload.Enabled = true;
            }

            globalFilters = _contextdynmanifest.ListFilters();

            DisplayAssetFilters();
            oktobuildlocator = true;
            BuildLocatorsTree();

        }

        private void DisplayAssetFilters()
        {

            dataGridViewFilters.ColumnCount = 7;
            dataGridViewFilters.Columns[0].HeaderText = "Name";
            dataGridViewFilters.Columns[0].Name = "Name";
            dataGridViewFilters.Columns[1].HeaderText = "Id";
            dataGridViewFilters.Columns[1].Name = "Id";
            dataGridViewFilters.Columns[2].HeaderText = "Track Rules";
            dataGridViewFilters.Columns[2].Name = "Rules";
            dataGridViewFilters.Columns[3].HeaderText = "Start (d.h:m:s)";
            dataGridViewFilters.Columns[3].Name = "Start";
            dataGridViewFilters.Columns[4].HeaderText = "End (d.h:m:s)";
            dataGridViewFilters.Columns[4].Name = "End";
            dataGridViewFilters.Columns[5].HeaderText = "DVR (d.h:m:s)";
            dataGridViewFilters.Columns[5].Name = "DVR";
            dataGridViewFilters.Columns[6].HeaderText = "Live backoff (d.h:m:s)";
            dataGridViewFilters.Columns[6].Name = "LiveBackoff";

            dataGridViewFilters.Rows.Clear();
            comboBoxLocatorsFilters.Items.Clear(); //drop list in locator tab
            comboBoxLocatorsFilters.BeginUpdate();
            comboBoxLocatorsFilters.Items.Add(new Item(string.Empty, null));

            List<AssetFilter> filters = _contextdynmanifest.ListAssetFilters(myAsset);

            if (filters.Count > 0 && myassetmanifesttimingdata == null)
            {
                myassetmanifesttimingdata = AssetInfo.GetManifestTimingData(myAsset);
            }



            foreach (var filter in filters)
            {
                string s = null;
                string e = null;
                string d = null;
                string l = null;

                if (filter.PresentationTimeRange != null)
                {
                    long start = Int64.Parse(filter.PresentationTimeRange.StartTimestamp) - myassetmanifesttimingdata.TimestampOffset;
                    long end = Int64.Parse(filter.PresentationTimeRange.EndTimestamp) - myassetmanifesttimingdata.TimestampOffset;
                    long dvr = Int64.Parse(filter.PresentationTimeRange.PresentationWindowDuration);
                    long live = Int64.Parse(filter.PresentationTimeRange.LiveBackoffDuration);

                    double scale = Convert.ToDouble(filter.PresentationTimeRange.Timescale) / 10000000;
                    e = (end == long.MaxValue) ? "max" : TimeSpan.FromTicks((long)(end / scale)).ToString(@"d\.hh\:mm\:ss");
                    s = (start == long.MaxValue) ? "max" : TimeSpan.FromTicks((long)(start / scale)).ToString(@"d\.hh\:mm\:ss");
                    d = (dvr == long.MaxValue) ? "max" : TimeSpan.FromTicks((long)(dvr / scale)).ToString(@"d\.hh\:mm\:ss");
                    l = (live == long.MaxValue) ? "max" : TimeSpan.FromTicks((long)(live / scale)).ToString(@"d\.hh\:mm\:ss");
                }
                int rowi = dataGridViewFilters.Rows.Add(filter.Name, filter.Id, filter.Tracks.Count, s, e, d, l);

                // droplist
                comboBoxLocatorsFilters.Items.Add(new Item("Asset filter  : " + filter.Name, filter.Name));
            }

            globalFilters.ForEach(g => comboBoxLocatorsFilters.Items.Add(new Item("Global filter : " + g.Name, g.Name)));
            comboBoxLocatorsFilters.SelectedIndex = 0;
            comboBoxLocatorsFilters.EndUpdate();
        }

        private IStreamingEndpoint ReturnSelectedStreamingEndpoint()
        {
            if (comboBoxStreamingEndpoint.SelectedItem != null)
            {
                string hostname = ((Item)comboBoxStreamingEndpoint.SelectedItem).Value;
                return myStreamingEndpoints.Where(se => se.HostName == hostname).FirstOrDefault();

            }
            else return null;
        }


        private void BuildLocatorsTree()
        {
            // LOCATORS TREE
            if (!oktobuildlocator) return;

            IEnumerable<IAssetFile> MyAssetFiles;
            List<Uri> ProgressiveDownloadUris;
            IStreamingEndpoint SelectedSE = ReturnSelectedStreamingEndpoint();
            if (SelectedSE != null)
            {
                bool CurrentStreamingEndpointHasRUs = SelectedSE.ScaleUnits > 0;
                Color colornodeRU = CurrentStreamingEndpointHasRUs ? Color.Black : Color.Gray;
                string filter = ((Item)comboBoxLocatorsFilters.SelectedItem).Value;

                TreeViewLocators.BeginUpdate();
                TreeViewLocators.Nodes.Clear();
                int indexloc = -1;
                foreach (ILocator locator in myAsset.Locators)
                {
                    indexloc++;
                    Color colornode;
                    string locatorstatus = string.Empty;
                    string SEstatus = string.Empty;

                    switch (AssetInfo.GetPublishedStatusForLocator(locator))
                    {
                        case PublishStatus.PublishedActive:
                            colornode = Color.Black;
                            locatorstatus = "Active";
                            break;
                        case PublishStatus.PublishedExpired:
                            colornode = Color.Red;
                            locatorstatus = "Expired";
                            break;
                        case PublishStatus.PublishedFuture:
                            colornode = Color.Blue;
                            locatorstatus = "Future";
                            break;
                        default:
                            colornode = Color.Black;
                            break;
                    }
                    if (SelectedSE.State != StreamingEndpointState.Running) colornode = Color.Red;

                    TreeNode myLocNode = new TreeNode(string.Format("{0} ({1}{2}) {3}", locator.Type.ToString(), locatorstatus, (SelectedSE.State != StreamingEndpointState.Running) ? ", Endpoint Stopped" : string.Empty, locator.Name));
                    myLocNode.ForeColor = colornode;

                    TreeViewLocators.Nodes.Add(myLocNode);

                    TreeViewLocators.Nodes[indexloc].Nodes.Add(new TreeNode("Locator information"));

                    TreeViewLocators.Nodes[indexloc].Nodes[0].Nodes.Add(new TreeNode(
                   string.Format("{0}", (locator.Id))
                   ));

                    TreeViewLocators.Nodes[indexloc].Nodes[0].Nodes.Add(new TreeNode(
                        string.Format("Name: {0}", locator.Name)
                        ));

                    TreeViewLocators.Nodes[indexloc].Nodes[0].Nodes.Add(new TreeNode(
                        string.Format("Type: {0}", locator.Type.ToString())
                        ));

                    if (locator.StartTime != null)
                        TreeViewLocators.Nodes[indexloc].Nodes[0].Nodes.Add(new TreeNode(
                           string.Format("Start time: {0}", (((DateTime)locator.StartTime).ToLocalTime().ToString()))
                           ));

                    if (locator.ExpirationDateTime != null)
                        TreeViewLocators.Nodes[indexloc].Nodes[0].Nodes.Add(new TreeNode(
                         string.Format("Expiration date time: {0}", (((DateTime)locator.ExpirationDateTime).ToLocalTime().ToString()))
                         ));

                    if (locator.Type == LocatorType.OnDemandOrigin)
                    {
                        TreeViewLocators.Nodes[indexloc].Nodes[0].Nodes.Add(new TreeNode(
                     string.Format("Path: {0}", AssetInfo.RW(locator.Path, SelectedSE))
                     ));

                        int indexn = 1;

                        TreeViewLocators.Nodes[indexloc].Nodes.Add(new TreeNode(AssetInfo._prog_down_http_streaming) { ForeColor = colornodeRU });
                        foreach (IAssetFile IAF in myAsset.AssetFiles)
                            TreeViewLocators.Nodes[indexloc].Nodes[indexn].Nodes.Add(new TreeNode((new Uri(AssetInfo.RW(locator.Path, SelectedSE, null, checkBoxHttps.Checked) + IAF.Name)).AbsoluteUri) { ForeColor = colornodeRU });
                        indexn++;

                        if (myAsset.AssetType == AssetType.MediaServicesHLS)
                        // It is a static HLS asset, so let's propose only the standard HLS V3 locator
                        {
                            TreeViewLocators.Nodes[indexloc].Nodes.Add(new TreeNode(AssetInfo._hls));
                            TreeViewLocators.Nodes[indexloc].Nodes[indexn].Nodes.Add(new TreeNode(locator.GetHlsUri().AbsoluteUri));
                            indexn++;
                        }
                        else if (myAsset.AssetType == AssetType.SmoothStreaming || myAsset.AssetType == AssetType.MultiBitrateMP4 || myAsset.AssetType == AssetType.Unknown) //later to change Unknown to live archive
                        // It's not Static HLS
                        // Smooth or multi MP4
                        {
                            if (locator.GetSmoothStreamingUri() != null)
                            {
                                Color ColorSmooth = ((myAsset.AssetType == AssetType.SmoothStreaming) && !checkBoxHttps.Checked) ? Color.Black : colornodeRU; // if not RU but aset is smooth, we can display the smooth URL as OK. If user asked for https, it works only with RU
                                TreeViewLocators.Nodes[indexloc].Nodes.Add(new TreeNode(AssetInfo._smooth) { ForeColor = ColorSmooth });
                                foreach (var uri in AssetInfo.GetSmoothStreamingUris(locator, SelectedSE, filter, checkBoxHttps.Checked))
                                {
                                    TreeViewLocators.Nodes[indexloc].Nodes[indexn].Nodes.Add(new TreeNode(uri.AbsoluteUri) { ForeColor = ColorSmooth });
                                }
                                indexn++;

                                TreeViewLocators.Nodes[indexloc].Nodes.Add(new TreeNode(AssetInfo._smooth_legacy) { ForeColor = colornodeRU });
                                foreach (var uri in AssetInfo.GetSmoothStreamingLegacyUris(locator, SelectedSE, filter, checkBoxHttps.Checked))
                                {
                                    TreeViewLocators.Nodes[indexloc].Nodes[indexn].Nodes.Add(new TreeNode(uri.AbsoluteUri) { ForeColor = colornodeRU });
                                }
                                indexn++;
                            }
                            if (locator.GetMpegDashUri() != null)
                            {
                                TreeViewLocators.Nodes[indexloc].Nodes.Add(new TreeNode(AssetInfo._dash) { ForeColor = colornodeRU });
                                foreach (var uri in AssetInfo.GetMpegDashUris(locator, SelectedSE, filter, checkBoxHttps.Checked))
                                {
                                    TreeViewLocators.Nodes[indexloc].Nodes[indexn].Nodes.Add(new TreeNode(uri.AbsoluteUri) { ForeColor = colornodeRU });
                                }
                                indexn++;
                            }
                            if (locator.GetHlsUri() != null)
                            {
                                TreeViewLocators.Nodes[indexloc].Nodes.Add(new TreeNode(AssetInfo._hls_v4) { ForeColor = colornodeRU });
                                foreach (var uri in AssetInfo.GetHlsUris(locator, SelectedSE, filter, checkBoxHttps.Checked))
                                {
                                    TreeViewLocators.Nodes[indexloc].Nodes[indexn].Nodes.Add(new TreeNode(uri.AbsoluteUri) { ForeColor = colornodeRU });
                                }
                                TreeViewLocators.Nodes[indexloc].Nodes.Add(new TreeNode(AssetInfo._hls_v3) { ForeColor = colornodeRU });
                                foreach (var uri in AssetInfo.GetHlsv3Uris(locator, SelectedSE, filter, checkBoxHttps.Checked))
                                {
                                    TreeViewLocators.Nodes[indexloc].Nodes[indexn + 1].Nodes.Add(new TreeNode(uri.AbsoluteUri) { ForeColor = colornodeRU });
                                }
                                indexn = indexn + 2;
                            }
                        }
                    }

                    if (locator.Type == LocatorType.Sas)
                    {
                        TreeViewLocators.Nodes[indexloc].Nodes[0].Nodes.Add(new TreeNode(
                     string.Format("Path: {0}", locator.Path)
                     ));

                        TreeViewLocators.Nodes[indexloc].Nodes.Add(new TreeNode(AssetInfo._prog_down_https_SAS));

                        MyAssetFiles = myAsset
                     .AssetFiles
                     .ToList();

                        // Generate the Progressive Download URLs for each file. 
                        ProgressiveDownloadUris =
                            MyAssetFiles.Select(af => af.GetSasUri(locator)).ToList();
                        ProgressiveDownloadUris.ForEach(uri => TreeViewLocators.Nodes[indexloc].Nodes[1].Nodes.Add(new TreeNode(uri.AbsoluteUri)));
                    }
                }
                TreeViewLocators.EndUpdate();
            }

        }



        private void DoDisplayFileProperties()
        {
            if (listViewFiles.SelectedItems.Count > 0)
            {
                IAssetFile AF = myAsset.AssetFiles.Skip(listViewFiles.SelectedIndices[0]).Take(1).FirstOrDefault();
                DGFiles.Rows.Clear();
                DGFiles.Rows.Add("Name", AF.Name);
                DGFiles.Rows.Add("Id", AF.Id);
                DGFiles.Rows.Add("File size", AssetInfo.FormatByteSize(AF.ContentFileSize));
                DGFiles.Rows.Add("Mime type", AF.MimeType);
                DGFiles.Rows.Add("Created", AF.Created.ToLocalTime());
                DGFiles.Rows.Add("Last modified", AF.LastModified.ToLocalTime());
                DGFiles.Rows.Add("Primary file", AF.IsPrimary);
                DGFiles.Rows.Add("Encrypted", AF.IsEncrypted);
                DGFiles.Rows.Add("Encryption scheme", AF.EncryptionScheme);
                DGFiles.Rows.Add("Encryption version", AF.EncryptionVersion);
                DGFiles.Rows.Add("Encryption key id", AF.EncryptionKeyId);
                DGFiles.Rows.Add("Parent asset Id", AF.ParentAssetId);
            }
        }

        private void DoDisplayDeliveryPolicyProperties()
        {
            if (listViewDelPol.SelectedItems.Count > 0)
            {
                IAssetDeliveryPolicy ADP = myAsset.DeliveryPolicies.Skip(listViewDelPol.SelectedIndices[0]).Take(1).FirstOrDefault();
                DGDelPol.Rows.Clear();
                DGDelPol.Rows.Add("Name", ADP.Name);
                DGDelPol.Rows.Add("Id", ADP.Id);
                DGDelPol.Rows.Add("Type", ADP.AssetDeliveryPolicyType);
                DGDelPol.Rows.Add("Protocol", ADP.AssetDeliveryProtocol);
                if (ADP.AssetDeliveryConfiguration != null)
                {
                    foreach (var conf in ADP.AssetDeliveryConfiguration)
                    {
                        DGDelPol.Rows.Add("Configuration, Key", conf.Key);
                        DGDelPol.Rows.Add("Configuration, Value", conf.Value);
                    }
                }
            }
        }

        private void DoDisplayKeyProperties()
        {
            if (listViewKeys.SelectedItems.Count > 0)
            {
                IContentKey key = myAsset.ContentKeys.Skip(listViewKeys.SelectedIndices[0]).Take(1).FirstOrDefault();
                dataGridViewKeys.Rows.Clear();
                dataGridViewKeys.Rows.Add("Name", key.Name != null ? key.Name : "<no name>");
                dataGridViewKeys.Rows.Add("Id", key.Id);
                dataGridViewKeys.Rows.Add("Content key type", key.ContentKeyType);
                dataGridViewKeys.Rows.Add("Checksum", key.Checksum);
                dataGridViewKeys.Rows.Add("Created", key.Created.ToLocalTime());
                dataGridViewKeys.Rows.Add("Las modified", key.LastModified.ToLocalTime());
                dataGridViewKeys.Rows.Add("Protection key Id", key.ProtectionKeyId);
                dataGridViewKeys.Rows.Add("Protection key type", key.ProtectionKeyType);
                dataGridViewKeys.Rows.Add("GetClearKeyValue", Convert.ToBase64String(key.GetClearKeyValue()));

                listViewAutPolOptions.Items.Clear();
                dataGridViewAutPolOption.Rows.Clear();

                if (key.AuthorizationPolicyId != null)
                {
                    dataGridViewKeys.Rows.Add("Authorization Policy Id", key.AuthorizationPolicyId);
                    myAuthPolicy = myContext.ContentKeyAuthorizationPolicies.Where(p => p.Id == key.AuthorizationPolicyId).FirstOrDefault();
                    if (myAuthPolicy != null)
                    {
                        dataGridViewKeys.Rows.Add("Authorization Policy Name", myAuthPolicy.Name);
                        listViewAutPolOptions.BeginUpdate();
                        foreach (var option in myAuthPolicy.Options)
                        {
                            ListViewItem item = new ListViewItem((string.IsNullOrEmpty(myAuthPolicy.Name) ? "<no name>" : myAuthPolicy.Name) + " / " + (string.IsNullOrEmpty(option.Name) ? "<no name>" : option.Name), 0);
                            listViewAutPolOptions.Items.Add(item);
                        }
                        listViewAutPolOptions.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                        listViewAutPolOptions.EndUpdate();
                        if (listViewAutPolOptions.Items.Count > 0) listViewAutPolOptions.Items[0].Selected = true;
                    }
                }
                else
                {
                    myAuthPolicy = null;
                }

                switch (key.ContentKeyType)
                {

                    case ContentKeyType.CommonEncryption:
                        string DelUrl;
                        try
                        {
                            DelUrl = key.GetKeyDeliveryUrl(ContentKeyDeliveryType.PlayReadyLicense).OriginalString;
                        }
                        catch (Exception e) // Perhaps PlayReady license delivery has been activated
                        {
                            if (e.InnerException == null)
                            {
                                DelUrl = e.Message;
                            }
                            else
                            {
                                DelUrl = string.Format("{0} ({1})", e.Message, Program.GetErrorMessage(e));
                            }
                        }
                        dataGridViewKeys.Rows.Add("GetkeyDeliveryUrl", DelUrl);
                        break;

                    case ContentKeyType.EnvelopeEncryption:
                        dataGridViewKeys.Rows.Add("GetkeyDeliveryUrl", key.GetKeyDeliveryUrl(ContentKeyDeliveryType.BaselineHttp).OriginalString);
                        break;


                    default:
                        break;

                }
            }
            else
            {
                myAuthPolicy = null;
            }
        }

        private static string FormatXmlString(string xmlString)
        {

            if (string.IsNullOrEmpty(xmlString))
            {

                return xmlString;

            }

            else
            {

                System.Xml.Linq.XElement element = System.Xml.Linq.XElement.Parse(xmlString);

                return element.ToString();

            }

        }



        private void AssetInformation_FormClosed(object sender, FormClosedEventArgs e)
        {
            // let's delete temporary locators if any
            if (tempLocator != null)
            {
                try
                {
                    var locatorTask = Task.Factory.StartNew(() =>
                   {
                       tempLocator.Delete();
                   });
                    locatorTask.Wait();
                }
                catch
                {

                }
            }
            if (tempMetadaLocator != null)
            {
                try
                {
                    var locatorTask = Task.Factory.StartNew(() =>
                    {
                        tempMetadaLocator.Delete();
                    });
                    locatorTask.Wait();
                }
                catch
                {

                }
            }
        }

        private void toolStripMenuItemOpenFile_Click(object sender, EventArgs e)
        {
            DoOpenFile();
        }

        private void DoOpenFile()
        {
            if (listViewFiles.SelectedItems.Count > 0)
            {
                if (listViewFiles.SelectedItems[0] != null)
                {
                    IAssetFile AF = null;
                    ILocator locator = GetTemporaryLocator();

                    try
                    {
                        if (locator != null)
                        {
                            AF = myAsset.AssetFiles.Skip(listViewFiles.SelectedIndices[0]).Take(1).FirstOrDefault();
                            Process.Start(AF.GetSasUri(locator).ToString());
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Error when accessing temporary SAS locator");
                    }
                }
            }
        }

        private void toolStripMenuItemDownloadFile_Click(object sender, EventArgs e)
        {
            DoDownloadFile();
        }

        private void DoDownloadFile()
        {
            if (listViewFiles.SelectedItems.Count > 0)
            {
                IAssetFile AF = myAsset.AssetFiles.Skip(listViewFiles.SelectedIndices[0]).Take(1).FirstOrDefault();
                if (AF == null) return;

                if (folderBrowserDialogDownload.ShowDialog() == DialogResult.OK)
                {
                    int index = myMainForm.DoGridTransferAddItem(string.Format("Download of file '{0}' from asset '{1}'", AF.Name, myAsset.Name), TransferType.DownloadToLocal, Properties.Settings.Default.useTransferQueue);

                    // Start a worker thread that does downloading.
                    myMainForm.DoDownloadFileFromAsset(myAsset, AF, folderBrowserDialogDownload.SelectedPath, index);
                }
            }
        }

        private void buttonCopyStats_Click(object sender, EventArgs e)
        {
            DoAssetStats();

        }

        private void DoAssetStats()
        {

            AssetInfo MyAssetReport = new AssetInfo(myAsset);
            MyAssetReport.CopyStatsToClipBoard();
        }

        private void buttonCreateMail_Click(object sender, EventArgs e)
        {
            DoAssetCreateMail();
        }

        private void DoAssetCreateMail()
        {
            AssetInfo MyAssetReport = new AssetInfo(myAsset);
            MyAssetReport.CreateOutlookMail();
        }

        private void makeItPrimaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MakeItAsPrimary();
        }

        private void MakeItAsPrimary()
        {
            if (listViewFiles.SelectedItems.Count > 0)
            {
                if (listViewFiles.SelectedItems[0] != null)
                {
                    try
                    {
                        myAsset.AssetFiles.ToList().ForEach(af => { af.IsPrimary = false; af.Update(); });
                        IAssetFile AF = myAsset.AssetFiles.Skip(listViewFiles.SelectedIndices[0]).Take(1).FirstOrDefault();
                        AF.IsPrimary = true;
                        AF.Update();
                    }
                    catch
                    {
                        MessageBox.Show("Error when making this file primary");
                    }
                    ListAssetFiles();
                    DoDisplayFileProperties();
                }
            }
        }

        private void buttonSetPrimary_Click(object sender, EventArgs e)
        {
            MakeItAsPrimary();
        }

        private void buttonDeleteFile_Click(object sender, EventArgs e)
        {
            DoDeleteFile();
            BuildLocatorsTree();
        }

        private void DoDeleteFile()
        {
            if (listViewFiles.SelectedItems.Count > 0)
            {
                if (listViewFiles.SelectedItems[0] != null)
                {
                    try
                    {
                        IAssetFile AF = myAsset.AssetFiles.Skip(listViewFiles.SelectedIndices[0]).Take(1).FirstOrDefault();
                        string question = "Delete the file " + AF.Name + " ?";

                        if (System.Windows.Forms.MessageBox.Show(question, "File deletion", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                        {
                            AF.Delete();
                            ListAssetFiles();
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Error when deleting this file");
                        ListAssetFiles();
                    }

                }
            }
        }

        private void deleteFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoDeleteFile();
            BuildLocatorsTree();
        }

        private void buttonOpenFile_Click(object sender, EventArgs e)
        {
            DoOpenFile();
        }

        private void buttonDownloadFile_Click(object sender, EventArgs e)
        {
            DoDownloadFile();
        }

        private void listViewFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool bSelect = listViewFiles.SelectedItems.Count > 0 ? true : false;
            bool NonEncrypted = (myAsset.Options == AssetCreationOptions.None);
            buttonDeleteFile.Enabled = bSelect;
            buttonSetPrimary.Enabled = bSelect;
            buttonDownloadFile.Enabled = bSelect;
            buttonOpenFile.Enabled = bSelect;
            buttonDuplicate.Enabled = bSelect & NonEncrypted;
            buttonUpload.Enabled = true;
            buttonFileMetadata.Enabled = bSelect;
            DoDisplayFileProperties();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DoDASHIFPlayer();
        }

        private void buttonFlash_Click(object sender, EventArgs e)
        {
            DoFlashPlayer();
        }



        private void buttonHTML_Click(object sender, EventArgs e)
        {
            DoHTMLPlayer();
        }

        private void TreeViewLocators_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (TreeViewLocators.SelectedNode != null)
            {
                if (TreeViewLocators.SelectedNode.Parent != null)
                {
                    buttonDASH.Enabled = false;
                    buttonAzureMediaPlayer.Enabled = false;
                    buttonDashLiveAzure.Enabled = false;
                    buttonFlash.Enabled = false;
                    buttonSLMonitor.Enabled = false;
                    buttonHTML.Enabled = false;
                    buttonOpen.Enabled = false;
                    buttonDel.Enabled = false;

                    switch (TreeViewLocators.SelectedNode.Parent.Text)
                    {
                        case AssetInfo._smooth:
                        case AssetInfo._smooth_legacy:

                            buttonDASH.Enabled = false;
                            buttonAzureMediaPlayer.Enabled = true;
                            buttonDashLiveAzure.Enabled = false;
                            buttonFlash.Enabled = true;
                            buttonSLMonitor.Enabled = true;
                            buttonHTML.Enabled = false;
                            buttonOpen.Enabled = false;
                            break;

                        case AssetInfo._dash:
                            buttonDASH.Enabled = true;
                            buttonAzureMediaPlayer.Enabled = true;
                            buttonDashLiveAzure.Enabled = true;
                            buttonFlash.Enabled = true;
                            buttonSLMonitor.Enabled = false;
                            buttonHTML.Enabled = false;
                            buttonOpen.Enabled = false;
                            break;

                        case AssetInfo._prog_down_https_SAS:
                            buttonDASH.Enabled = false;
                            buttonAzureMediaPlayer.Enabled = (TreeViewLocators.SelectedNode.Text.ToLower().EndsWith(".mp4"));
                            buttonDashLiveAzure.Enabled = false;
                            buttonFlash.Enabled = false;
                            buttonSLMonitor.Enabled = false;
                            buttonHTML.Enabled = false;
                            buttonOpen.Enabled = true;
                            break;

                        case AssetInfo._prog_down_http_streaming:
                            buttonDASH.Enabled = false;
                            buttonAzureMediaPlayer.Enabled = (TreeViewLocators.SelectedNode.Text.ToLower().EndsWith(".mp4"));
                            buttonDashLiveAzure.Enabled = false;
                            buttonFlash.Enabled = false;
                            buttonSLMonitor.Enabled = false;
                            buttonHTML.Enabled = (TreeViewLocators.SelectedNode.Text.ToLower().EndsWith(".mp4"));
                            buttonOpen.Enabled = !(TreeViewLocators.SelectedNode.Text.ToLower().EndsWith(".ism"));
                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    buttonDel.Enabled = true; // parent is null, so we can propose to delete the locator
                }

            }
        }

        private void playbackWithToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoAzureMediaPlayer();
        }

        private void DoAzureMediaPlayer()
        {
            if (TreeViewLocators.SelectedNode != null)
            {
                // Root node's Parent property is null, so do check
                if (TreeViewLocators.SelectedNode.Parent != null)
                {
                    switch (TreeViewLocators.SelectedNode.Parent.Text)
                    {
                        case AssetInfo._dash:
                            AssetInfo.DoPlayBackWithBestStreamingEndpoint(typeplayer: PlayerType.AzureMediaPlayer, Urlstr: TreeViewLocators.SelectedNode.Text, DoNotRewriteURL: true, context: myContext, formatamp: AzureMediaPlayerFormats.Dash);

                            break;

                        case AssetInfo._smooth:
                        case AssetInfo._smooth_legacy:
                            AssetInfo.DoPlayBackWithBestStreamingEndpoint(typeplayer: PlayerType.AzureMediaPlayer, Urlstr: TreeViewLocators.SelectedNode.Text, DoNotRewriteURL: true, context: myContext, formatamp: AzureMediaPlayerFormats.Smooth);
                            break;

                        case AssetInfo._hls_v4:
                        case AssetInfo._hls_v3:
                        case AssetInfo._hls:
                            AssetInfo.DoPlayBackWithBestStreamingEndpoint(typeplayer: PlayerType.AzureMediaPlayer, Urlstr: TreeViewLocators.SelectedNode.Text, DoNotRewriteURL: true, context: myContext, formatamp: AzureMediaPlayerFormats.HLS);
                            break;

                        case AssetInfo._prog_down_http_streaming:
                        case AssetInfo._prog_down_https_SAS:
                            AssetInfo.DoPlayBackWithBestStreamingEndpoint(typeplayer: PlayerType.AzureMediaPlayer, Urlstr: TreeViewLocators.SelectedNode.Text, DoNotRewriteURL: true, context: myContext, formatamp: AzureMediaPlayerFormats.VideoMP4);
                            break;

                        default:
                            break;
                    }

                }
            }
        }

        private void buttonSLMonitor_Click(object sender, EventArgs e)
        {
            DoSLPlayer();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DoAzureMediaPlayer();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            DoDuplicate();
            BuildLocatorsTree();
        }

        private void DoDuplicate()
        {
            if (listViewFiles.SelectedItems.Count > 0)
            {
                if (listViewFiles.SelectedItems[0] != null)
                {
                    try
                    {
                        IAssetFile AF = myAsset.AssetFiles.Skip(listViewFiles.SelectedIndices[0]).Take(1).FirstOrDefault();

                        if (!Mainform.havestoragecredentials)
                        { // No blob credentials.
                            MessageBox.Show("Please specify the account storage key in the login window.");
                        }
                        else
                        {
                            string newfilename = "Copy of " + AF.Name;
                            if (Program.InputBox("New name", "Enter the name of the new duplicate file:", ref newfilename) == DialogResult.OK)
                            {
                                IAssetFile AFDup = myAsset.AssetFiles.Create(newfilename);
                                CloudMediaContext _context = Mainform._context;
                                CloudStorageAccount storageAccount;
                                storageAccount = new CloudStorageAccount(new StorageCredentials(_context.DefaultStorageAccount.Name, Mainform._credentials.StorageKey), Mainform._credentials.ReturnStorageSuffix(), true);
                                var cloudBlobClient = storageAccount.CreateCloudBlobClient();
                                IAccessPolicy writePolicy = _context.AccessPolicies.Create("writePolicy", TimeSpan.FromDays(1), AccessPermissions.Write);
                                ILocator destinationLocator = _context.Locators.CreateLocator(LocatorType.Sas, myAsset, writePolicy);

                                // Get the asset container URI and copy blobs from mediaContainer to assetContainer.
                                Uri uploadUri = new Uri(destinationLocator.Path);
                                string assetTargetContainerName = uploadUri.Segments[1];
                                CloudBlobContainer assetTargetContainer = cloudBlobClient.GetContainerReference(assetTargetContainerName);
                                var mediaBlobContainer = assetTargetContainer; // same container

                                CloudBlockBlob sourceCloudBlob, destinationBlob;

                                sourceCloudBlob = mediaBlobContainer.GetBlockBlobReference(AF.Name);
                                sourceCloudBlob.FetchAttributes();

                                if (sourceCloudBlob.Properties.Length > 0)
                                {

                                    destinationBlob = assetTargetContainer.GetBlockBlobReference(AFDup.Name);

                                    destinationBlob.DeleteIfExists();
                                    destinationBlob.StartCopyFromBlob(sourceCloudBlob);

                                    CloudBlockBlob blob;
                                    blob = (CloudBlockBlob)assetTargetContainer.GetBlobReferenceFromServer(AFDup.Name);

                                    while (blob.CopyState.Status == CopyStatus.Pending)
                                    {
                                        Task.Delay(TimeSpan.FromSeconds(1d)).Wait();
                                    }
                                    destinationBlob.FetchAttributes();
                                    AFDup.ContentFileSize = sourceCloudBlob.Properties.Length;
                                    AFDup.Update();

                                    myAsset.Update();

                                    destinationLocator.Delete();
                                    writePolicy.Delete();

                                    // Refresh the asset.
                                    myAsset = _context.Assets.Where(a => a.Id == myAsset.Id).FirstOrDefault();

                                }
                            }

                            ListAssetFiles();
                        }

                    }
                    catch
                    {
                        MessageBox.Show("Error when duplicating this file");
                        ListAssetFiles();
                    }

                }

            }
        }

        private async void DoUpload()
        {
            OpenFileDialog Dialog = new OpenFileDialog();
            Dialog.Multiselect = true;
            if (Dialog.ShowDialog() == DialogResult.OK)
            {
                progressBarUpload.Maximum = 100 * Dialog.FileNames.Count();
                progressBarUpload.Visible = true;
                buttonClose.Enabled = false;
                buttonUpload.Enabled = false;
                foreach (string file in Dialog.FileNames)
                {
                    await Task.Factory.StartNew(() => ProcessUploadFileToAsset(Path.GetFileName(file), file, myAsset));
                }
                // Refresh the asset.
                myAsset = Mainform._context.Assets.Where(a => a.Id == myAsset.Id).FirstOrDefault();
                progressBarUpload.Visible = false;
                buttonClose.Enabled = true;
                buttonUpload.Enabled = true;
                ListAssetFiles();
            }
        }

        private void ProcessUploadFileToAsset(string SafeFileName, string FileName, IAsset MyAsset)
        {
            try
            {
                IAssetFile UploadedAssetFile = MyAsset.AssetFiles.Create(SafeFileName);
                UploadedAssetFile.UploadProgressChanged += MyUploadProgressChanged;
                UploadedAssetFile.Upload(FileName as string);
            }
            catch
            {
                MessageBox.Show("Error when uploading the file");
            }
        }

        private void MyUploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            progressBarUpload.BeginInvoke(new Action(() => progressBarUpload.Value = (int)e.Progress), null);

        }



        private void duplicateFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoDuplicate();
            BuildLocatorsTree();
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            DoOpenFileLocator();
        }

        private void DoOpenFileLocator()
        {
            if (TreeViewLocators.SelectedNode != null)
            {
                // Root node's Parent property is null, so do check
                if (TreeViewLocators.SelectedNode.Parent != null)
                {
                    Process.Start(TreeViewLocators.SelectedNode.Text);

                }

            }
        }

        private void buttonDel_Click(object sender, EventArgs e)
        {
            DoDelLocator();
        }

        private void DoDelLocator()
        {
            if (TreeViewLocators.SelectedNode != null)
            {

                if (TreeViewLocators.SelectedNode.Parent == null)   // user selected the locator title
                {

                    if (System.Windows.Forms.MessageBox.Show("Are you sure that you want to delete this locator ?", "Locator deletion", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        bool Error = false;
                        try
                        {
                            myAsset.Locators[TreeViewLocators.SelectedNode.Index].Delete();
                        }

                        catch
                        {

                            MessageBox.Show("Error when trying to delete the locator.");
                            Error = true;
                        }
                        if (!Error) BuildLocatorsTree();
                    }
                }

            }
        }

        private void deleteLocatorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoDelLocator();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            DoUpload();
            BuildLocatorsTree();
        }

        private void uploadASmallFileInTheAssetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoUpload();
            BuildLocatorsTree();
        }

        private void comboBoxStreamingEndpoint_SelectedIndexChanged(object sender, EventArgs e)
        {
            BuildLocatorsTree();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void listViewDelPol_SelectedIndexChanged(object sender, EventArgs e)
        {

            bool bSelect = listViewDelPol.SelectedItems.Count > 0 ? true : false;
            buttonRemoveDelPol.Enabled = bSelect;
            DoDisplayDeliveryPolicyProperties();
        }

        private void buttonRemovePol_Click(object sender, EventArgs e)
        {
            DoRemoveDeliveryPol();
        }

        private void DoRemoveDeliveryPol()
        {
            if (listViewDelPol.SelectedItems.Count > 0)
            {
                if (listViewDelPol.SelectedItems[0] != null)
                {
                    IAssetDeliveryPolicy DP = myAsset.DeliveryPolicies.Skip(listViewDelPol.SelectedIndices[0]).Take(1).FirstOrDefault();
                    if (DP != null)
                    {
                        string DPid = DP.Id;
                        string question = string.Format("This will remove the policy '{0}' from the asset.\nDo you want to also DELETE the policy from the Azure Media Services account ?", DP.Name);
                        DialogResult DR = MessageBox.Show(question, "Delivery Policy removal", MessageBoxButtons.YesNoCancel);

                        if (DR == DialogResult.Yes || DR == DialogResult.No)
                        {
                            string step = "removing";

                            try
                            {
                                myAsset.DeliveryPolicies.Remove(DP);

                                if (DR == DialogResult.Yes) // user wants also to delete the policy
                                {
                                    step = "deleting";
                                    IAssetDeliveryPolicy policyrefreshed = myContext.AssetDeliveryPolicies.Where(p => p.Id == DPid).FirstOrDefault();
                                    if (policyrefreshed != null)
                                    {
                                        policyrefreshed.Delete();
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                string messagestr = string.Format("Error when {0} the delivery policy.", step);
                                if (e.InnerException != null)
                                {
                                    messagestr += Constants.endline + Program.GetErrorMessage(e);
                                }
                                MessageBox.Show(messagestr);
                            }
                            ListAssetDeliveryPolicies();
                        }
                    }
                }
            }
        }

        private void listViewKeys_SelectedIndexChanged(object sender, EventArgs e)
        {
            buttonRemoveKey.Enabled = listViewKeys.SelectedItems.Count > 0;
            buttonRemoveAuthPol.Enabled = buttonGetTestToken.Enabled = false;
            DoDisplayKeyProperties();
        }


        private void listViewAutPolOption_SelectedIndexChanged(object sender, EventArgs e)
        {
            DoDisplayAuthorizationPolicyOption();
        }

        private void DoDisplayAuthorizationPolicyOption()
        {
            bool DisplayButGetToken = false;

            if (listViewAutPolOptions.SelectedItems.Count > 0)
            {
                dataGridViewAutPolOption.Rows.Clear();

                IContentKeyAuthorizationPolicyOption option = myAuthPolicy.Options.Skip(listViewAutPolOptions.SelectedIndices[0]).Take(1).FirstOrDefault();
                if (option != null) // Token option
                {

                    dataGridViewAutPolOption.Rows.Add("Name", option.Name != null ? option.Name : "<no name>");
                    dataGridViewAutPolOption.Rows.Add("Id", option.Id);
                    dataGridViewAutPolOption.Rows.Add("KeyDeliveryConfiguration", FormatXmlString(option.KeyDeliveryConfiguration));
                    dataGridViewAutPolOption.Rows.Add("KeyDeliveryType", option.KeyDeliveryType);
                    List<ContentKeyAuthorizationPolicyRestriction> objList_restriction = option.Restrictions;
                    foreach (var restriction in objList_restriction)
                    {
                        dataGridViewAutPolOption.Rows.Add("Restriction Name", restriction.Name);
                        dataGridViewAutPolOption.Rows.Add("Restriction KeyRestrictionType", (ContentKeyRestrictionType)restriction.KeyRestrictionType);
                        if ((ContentKeyRestrictionType)restriction.KeyRestrictionType == ContentKeyRestrictionType.TokenRestricted)
                        {
                            DisplayButGetToken = true;
                        }
                        if (restriction.Requirements != null)
                        {
                            dataGridViewAutPolOption.Rows.Add("Restriction Requirements", FormatXmlString(restriction.Requirements));
                            TokenRestrictionTemplate tokenTemplate = TokenRestrictionTemplateSerializer.Deserialize(restriction.Requirements);
                            dataGridViewAutPolOption.Rows.Add("Token Type", tokenTemplate.TokenType);
                            dataGridViewAutPolOption.Rows.Add("Token Verification Key Type", (tokenTemplate.PrimaryVerificationKey.GetType() == typeof(SymmetricVerificationKey)) ? "Symmetric" : "Asymmetric (X509)");
                            dataGridViewAutPolOption.Rows.Add("Token Audience", tokenTemplate.Audience);
                            dataGridViewAutPolOption.Rows.Add("Token Issuer", tokenTemplate.Issuer);
                            foreach (var claim in tokenTemplate.RequiredClaims)
                            {
                                dataGridViewAutPolOption.Rows.Add("Required Claim, Type", claim.ClaimType);
                                dataGridViewAutPolOption.Rows.Add("Required Claim, Value", claim.ClaimValue);
                            }
                        }
                    }
                }



            }
            buttonGetTestToken.Enabled = DisplayButGetToken;
            buttonRemoveAuthPol.Enabled = true;
        }

        private void buttonGetTestToken_Click(object sender, EventArgs e)
        {
            DoGetTestToken();
        }

        private void DoGetTestToken()
        {
            bool Error = true;
            if (listViewKeys.SelectedItems.Count > 0)
            {
                IContentKey key = myAsset.ContentKeys.Skip(listViewKeys.SelectedIndices[0]).Take(1).FirstOrDefault();
                if (key != null)
                {
                    IContentKeyAuthorizationPolicy AutPol = myContext.ContentKeyAuthorizationPolicies.Where(a => a.Id == key.AuthorizationPolicyId).FirstOrDefault();
                    if (AutPol != null)
                    {
                        IContentKeyAuthorizationPolicyOption AutPolOption = AutPol.Options.Skip(listViewAutPolOptions.SelectedIndices[0]).FirstOrDefault();
                        if (AutPolOption != null)
                        {
                            DynamicEncryption.TokenResult testToken = DynamicEncryption.GetTestToken(myAsset, myContext, key.ContentKeyType, displayUI: true, optionid: AutPolOption.Id);
                            if (!string.IsNullOrEmpty(testToken.TokenString))
                            {
                                myMainForm.TextBoxLogWriteLine("The authorization test token (without Bearer) is :\n{0}", testToken);
                                myMainForm.TextBoxLogWriteLine("The authorization test token (with Bearer) is :\n{0}", Constants.Bearer + testToken);
                                System.Windows.Forms.Clipboard.SetText(Constants.Bearer + testToken.TokenString);
                                MessageBox.Show(string.Format("The test token below has been be copied to the log window and clipboard.\n\n{0}", Constants.Bearer + testToken.TokenString), "Test token copied");
                                Error = false;
                            }
                        }
                    }
                }
            }
        }

        private void buttonDashLiveAzure_Click(object sender, EventArgs e)
        {
            DoDashLiveAzurePlayer();
        }

        private void DoDashLiveAzurePlayer()
        {
            if (TreeViewLocators.SelectedNode != null)
            {
                // Root node's Parent property is null, so do check
                if (TreeViewLocators.SelectedNode.Parent != null)
                {
                    switch (TreeViewLocators.SelectedNode.Parent.Text)
                    {
                        case AssetInfo._dash:
                            AssetInfo.DoPlayBackWithBestStreamingEndpoint(typeplayer: PlayerType.DASHLiveAzure, Urlstr: TreeViewLocators.SelectedNode.Text, DoNotRewriteURL: true, context: myContext);
                            break;


                        default:
                            break;
                    }

                }
            }
        }

        private void checkBoxHttps_CheckedChanged(object sender, EventArgs e)
        {
            BuildLocatorsTree();
        }

        private void buttonAudioVideoAnalysis_Click(object sender, EventArgs e)
        {
            IEnumerable<AssetFileMetadata> manifestAssetFile = myAsset.GetMetadata();

            IAssetFile metadatafile = myContext.Files.Where(f => f.Name == myAsset.Id.Replace(Constants.AssetIdPrefix, string.Empty) + "_metadata.xml").OrderBy(f => f.LastModified).FirstOrDefault();
            if (metadatafile != null)
            {
                bool Error = false;
                if (tempMetadaLocator == null)
                {
                    try
                    {
                        var locatorTask = Task.Factory.StartNew(() =>
                        {
                            tempMetadaLocator = myContext.Locators.Create(LocatorType.Sas, metadatafile.Asset, AccessPermissions.Read, TimeSpan.FromHours(1));
                        });
                        locatorTask.Wait();
                    }
                    catch
                    {
                        Error = true;
                        MessageBox.Show("Error when creating the temporary SAS locator to the metadata file.");
                    }
                }

                try
                {
                    if (!Error)
                    {

                        AssetFileMetadata MyAssetMetada = metadatafile.GetMetadata(tempMetadaLocator);

                    }
                }
                catch
                {
                    MessageBox.Show("Error when accessing temporary SAS locator");
                }


            }
        }

        private void buttonFileMetadata_Click(object sender, EventArgs e)
        {
            ShowFileMetadata();
        }

        private void ShowFileMetadata()
        {
            if (listViewFiles.SelectedItems.Count > 0)
            {
                if (listViewFiles.SelectedItems[0] != null)
                {
                    IAssetFile AF = null;
                    ILocator locator = GetTemporaryLocator();

                    if (locator != null)
                    {
                        AssetFileMetadata manifestAssetFile = null;
                        try
                        {
                            AF = myAsset.AssetFiles.Skip(listViewFiles.SelectedIndices[0]).Take(1).FirstOrDefault();
                            // Get the metadata for the asset file.
                            manifestAssetFile = AF.GetMetadata(locator);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error when accessing metadata." + ex.Message);
                        }

                        if (manifestAssetFile != null)
                        {
                            MetadataInformation form = new MetadataInformation(manifestAssetFile);
                            form.ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show("There is no metadata for this file.");
                        }
                    }
                }
            }
        }

        private ILocator GetTemporaryLocator()
        {
            if (tempLocator == null) // no temp locator, let's create it
            {
                try
                {
                    var locatorTask = Task.Factory.StartNew(() =>
                    {
                        tempLocator = myContext.Locators.Create(LocatorType.Sas, myAsset, AccessPermissions.Read, TimeSpan.FromHours(1));

                    });
                    locatorTask.Wait();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error when creating the temporary SAS locator." + ex.Message);
                }
            }
            return tempLocator;
        }


        private void contextMenuStripDG_MouseClick_1(object sender, MouseEventArgs e)
        {
            ContextMenuStrip contextmenu = (ContextMenuStrip)sender;
            DataGridView DG = (DataGridView)contextmenu.SourceControl;

            if (DG.SelectedCells.Count == 1)
            {
                if (DG.SelectedCells[0].Value != null)
                {
                    System.Windows.Forms.Clipboard.SetText(DG.SelectedCells[0].Value.ToString());

                }
                else
                {
                    System.Windows.Forms.Clipboard.Clear();
                }

            }
        }

        private void showMetadataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowFileMetadata();
        }

        private void buttonDelKey_Click(object sender, EventArgs e)
        {
            DoDemoveKey();
        }

        private void DoDemoveKey()
        {
            if (listViewKeys.SelectedItems.Count > 0)
            {
                IContentKey key = myAsset.ContentKeys.Skip(listViewKeys.SelectedIndices[0]).Take(1).FirstOrDefault();
                string keyid = key.Id;
                string question = string.Format("This will remove the key '{0}' from the asset.\nDo you want to also DELETE the key from the Azure Media Services account ?", key.Name);
                DialogResult DR = MessageBox.Show(question, "Key removal", MessageBoxButtons.YesNoCancel);

                if (DR == DialogResult.Yes || DR == DialogResult.No)
                {
                    string step = "removing";
                    try
                    {
                        myAsset.ContentKeys.Remove(key);
                        if (DR == DialogResult.Yes) // user wants also to delete the key
                        {
                            step = "deleting";
                            IContentKey keyrefreshed = myContext.ContentKeys.Where(k => k.Id == keyid).FirstOrDefault();
                            if (keyrefreshed != null)
                            {
                                keyrefreshed.Delete();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        string messagestr = string.Format("Error when {0} the key", step);
                        if (e.InnerException != null)
                        {
                            messagestr += Constants.endline + Program.GetErrorMessage(e);
                        }
                        MessageBox.Show(messagestr);
                    }
                    ListAssetKeys();
                }
            }
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoDemoveKey();
        }

        private void getTestTokenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoGetTestToken();
        }

        private void removeDeliveryPolicyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoRemoveDeliveryPol();
        }

        private void contextMenuStripDelPol_Opening(object sender, CancelEventArgs e)
        {
            removeDeliveryPolicyToolStripMenuItem.Enabled = (listViewDelPol.SelectedItems.Count > 0);
        }

        private void contextMenuStripKey_Opening(object sender, CancelEventArgs e)
        {
            removeKeyToolStripMenuItem.Enabled = (listViewKeys.SelectedItems.Count > 0);

        }

        private void contextMenuStripAuthPol_Opening(object sender, CancelEventArgs e)
        {
            getTestTokenToolStripMenuItem.Enabled = (listViewAutPolOptions.SelectedItems.Count > 0);
        }

        private void contextMenuStripFiles_Opening(object sender, CancelEventArgs e)
        {
            bool selected = (listViewFiles.SelectedItems.Count > 0);
            makeItPrimaryToolStripMenuItem.Enabled = selected;
            showMetadataToolStripMenuItem.Enabled = selected;
            toolStripMenuItemOpenFile.Enabled = selected;
            toolStripMenuItemDownloadFile.Enabled = selected;
            deleteFileToolStripMenuItem.Enabled = selected;
            duplicateFileToolStripMenuItem.Enabled = selected;
        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            DoRemoveAuthPol();
        }

        private void DoRemoveAuthPol()
        {
            if (listViewKeys.SelectedItems.Count > 0)
            {
                if (listViewKeys.SelectedItems[0] != null)
                {
                    IContentKey key = myAsset.ContentKeys.Skip(listViewKeys.SelectedIndices[0]).Take(1).FirstOrDefault();
                    IContentKeyAuthorizationPolicy AuthPol = myContext.ContentKeyAuthorizationPolicies.Where(p => p.Id == key.AuthorizationPolicyId).FirstOrDefault();

                    if (AuthPol != null)
                    {
                        string AuthPolId = AuthPol.Id;
                        string question = string.Format("This will remove the authorization policy '{0}' from the key.\nDo you want to also DELETE the policy from the Azure Media Services account ?", AuthPol.Name);
                        DialogResult DR = MessageBox.Show(question, "Delivery Policy removal", MessageBoxButtons.YesNoCancel);

                        if (DR == DialogResult.Yes || DR == DialogResult.No)
                        {
                            string step = "removing";
                            try
                            {
                                key.AuthorizationPolicyId = null;

                                if (DR == DialogResult.Yes) // user wants also to delete the auth policy
                                {
                                    step = "deleting";
                                    AuthPol.Delete();
                                }
                            }
                            catch (Exception e)
                            {
                                string messagestr = string.Format("Error when {0} the authorization policy.", step);
                                if (e.InnerException != null)
                                {
                                    messagestr += Constants.endline + Program.GetErrorMessage(e);
                                }
                                MessageBox.Show(messagestr);
                            }

                            DoDisplayKeyProperties();
                        }
                    }
                }
            }
        }

        private void filterInfoupdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoFilterInfo();
        }
        private List<AssetFilter> ReturnSelectedFilters()
        {

            List<AssetFilter> SelectedFilters = new List<AssetFilter>();
            foreach (DataGridViewRow Row in dataGridViewFilters.SelectedRows)
            {
                string filterid = Row.Cells[dataGridViewFilters.Columns["Id"].Index].Value.ToString();
                AssetFilter myfilter = _contextdynmanifest.GetAssetFilter(filterid);
                if (myfilter != null)
                {
                    SelectedFilters.Add(myfilter);
                }
            }
            return SelectedFilters;
        }
        private void DoFilterInfo()
        {
            var filters = ReturnSelectedFilters();
            if (filters.Count == 1)
            {
                DynManifestFilter form = new DynManifestFilter(_contextdynmanifest, myContext, (Filter)filters.FirstOrDefault(), myAsset);

                if (form.ShowDialog() == DialogResult.OK)
                {
                    AssetFilter filtertoupdate = (AssetFilter)form.GetFilter;
                    try
                    {
                        filtertoupdate.Delete();
                        filtertoupdate.Create();
                        myMainForm.TextBoxLogWriteLine("Asset filter '{0}' has been updated.", filtertoupdate.Name);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Error when updating asset filter.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        myMainForm.TextBoxLogWriteLine("Error when updating asset filter '{0}'.", filtertoupdate.Name, true);
                        myMainForm.TextBoxLogWriteLine(e);
                    }
                    DisplayAssetFilters();
                }
            }
        }

        private void createAnAssetFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoCreateAssetFilter();
        }

        private void DoCreateAssetFilter()
        {
            DynManifestFilter form = new DynManifestFilter(_contextdynmanifest, myContext, null, myAsset);
            form.CreateAssetFilterFromAssetName = myAsset.Name;

            if (form.ShowDialog() == DialogResult.OK)
            {
                AssetFilter myassetfilter = new AssetFilter(myAsset);

                Filter filter = form.GetFilter;
                myassetfilter.Name = filter.Name;
                myassetfilter.PresentationTimeRange = filter.PresentationTimeRange;
                myassetfilter.Tracks = filter.Tracks;
                myassetfilter._context = filter._context;
                try
                {
                    myassetfilter.Create();
                    myMainForm.TextBoxLogWriteLine("Asset filter '{0}' has been created.", filter.Name);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error when creating asset filter.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    myMainForm.TextBoxLogWriteLine("Error when creating asset filter '{0}'.", filter.Name, true);
                    myMainForm.TextBoxLogWriteLine(e);
                }
                DisplayAssetFilters();
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoDeleteAssetFilter();
        }

        private void DoDeleteAssetFilter()
        {
            var filters = ReturnSelectedFilters();
            try
            {
                filters.ForEach(f => f.Delete());
            }

            catch (Exception e)
            {
                MessageBox.Show("Error when deleting asset filter(s).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            DisplayAssetFilters();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DoDuplicateFilter();
        }

        private void DoDuplicateFilter()
        {
            var filters = ReturnSelectedFilters();
            if (filters.Count == 1)
            {
                AssetFilter sourcefilter = filters.FirstOrDefault();

                string newfiltername = sourcefilter.Name + "Copy";
                if (Program.InputBox("New name", "Enter the name of the new duplicate filter:", ref newfiltername) == DialogResult.OK)
                {
                    AssetFilter copyfilter = new AssetFilter(myAsset);
                    copyfilter.Name = newfiltername;
                    copyfilter.PresentationTimeRange = sourcefilter.PresentationTimeRange;
                    copyfilter.Tracks = sourcefilter.Tracks;
                    copyfilter._context = sourcefilter._context;
                    try
                    {
                        copyfilter.Create();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Error when duplicating asset filter.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    DisplayAssetFilters();
                }
            }
        }

        private void duplicateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoDuplicateFilter();
        }

        private void dataGridViewFilters_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            DoFilterInfo();
        }

        private void comboBoxLocatorsFilters_SelectedIndexChanged(object sender, EventArgs e)
        {
            BuildLocatorsTree();
        }

        private void button1_Click_3(object sender, EventArgs e)
        {
            DoFilterInfo();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            DoCreateAssetFilter();
        }

        private void buttonDeleteFilter_Click(object sender, EventArgs e)
        {
            DoDeleteAssetFilter();
        }

        private void button1_Click_4(object sender, EventArgs e)
        {
            DoPlayWithFilter();
        }

        private void DoPlayWithFilter()
        {
            myMainForm.DoPlaySelectedAssetsOrProgramsWithPlayer(PlayerType.AzureMediaPlayer, new List<IAsset>() { myAsset }, ReturnSelectedFilters().FirstOrDefault().Name);

        }

        private void playWithThisFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoPlayWithFilter();
        }
    }
}
