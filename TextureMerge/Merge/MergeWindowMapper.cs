using System.Windows;
using System.Windows.Controls;

namespace TextureMerge
{
    public partial class MainWindow : Window
    {
        private MergeWindowMapper mapper;

        private void MapResources()
        {
            mapper = new MergeWindowMapper()
            {
                slots = new MergeWindowMapper.SlotMapper[]
                {
                    // RED
                    new MergeWindowMapper.SlotMapper()
                    {
                        sourceChannelButton = new Button[]
                        {
                            srcRR, srcRG, srcRB,
                        },
                        loadButton = LoadR,
                        label = redNoDataLabel,
                        image = RedCh,
                        grayscaleSourceGrid = srcGridGsR,
                        colorSourceGrid = srcGridCR,
                    },

                    // GREEN
                    new MergeWindowMapper.SlotMapper()
                    {
                        sourceChannelButton = new Button[]
                        {
                            srcGR, srcGG, srcGB,
                        },
                        loadButton = LoadG,
                        label = greenNoDataLabel,
                        image = GreenCh,
                        grayscaleSourceGrid = srcGridGsG,
                        colorSourceGrid = srcGridCG,
                    },

                    // BLUE
                    new MergeWindowMapper.SlotMapper()
                    {
                        sourceChannelButton = new Button[]
                        {
                            srcBR, srcBG, srcBB,
                        },
                        loadButton = LoadB,
                        label = blueNoDataLabel,
                        image = BlueCh,
                        grayscaleSourceGrid = srcGridGsB,
                        colorSourceGrid = srcGridCB,
                    },

                    // ALPHA
                    new MergeWindowMapper.SlotMapper()
                    {
                        sourceChannelButton = new Button[]
                        {
                            srcAR, srcAG, srcAB,
                        },
                        loadButton = LoadA,
                        label = alphaNoDataLabel,
                        image = AlphaCh,
                        grayscaleSourceGrid = srcGridGsA,
                        colorSourceGrid = srcGridCA,
                    },
                }
            };
        }

        internal struct MergeWindowMapper
        {
            internal SlotMapper[] slots;

            internal struct SlotMapper
            {
                internal Button[] sourceChannelButton;
                internal Button loadButton;
                internal Label label;
                internal Image image;
                internal Grid grayscaleSourceGrid;
                internal Grid colorSourceGrid;
            }
        }
    }
}
