using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockRoot : MonoBehaviour
{
    public GameObject BlockPrefab = null;           // ���� ������ ������
    public BlockControl[,] blocks;                  // �׸���

    private GameObject main_camera = null;          // ���� ī�޶�
    private BlockControl grabbed_block = null;      // ���� ����

    private ScoreCounter score_counter = null;      // ���� ī���� 
    protected bool is_vanishing_prev = false;       // �տ��� ��ȭ�ߴ���

    public TextAsset levelData = null;              // ���� �������� �ؽ�Ʈ ����
    public LevelControl level_control;              // LevelControl ����

    // Start is called before the first frame update
    void Start()
    {
        // ī�޶�κ��� ���콺 Ŀ���� ����ϴ� ������ ��� ���� ����ī�޶� �ʿ�
        this.main_camera = GameObject.FindGameObjectWithTag("MainCamera");
        this.score_counter = this.gameObject.GetComponent<ScoreCounter>();
    }

    // Update is called once per frame
    void Update()
    {
        /// ���콺 ��ǥ�� ��ġ���� Ȯ��
        Vector3 mouse_position;     // ���콺 ��ġ
        this.UnprojectMousePosition(out mouse_position, Input.mousePosition);   // ���콺 ��ġ�� ������
        // ������ ���콺 ��ġ�� �ϳ��� Vector2�� ����
        Vector2 mouse_position_xy = new Vector2(mouse_position.x, mouse_position.y);

        /// ���� �� �ִ� ������ ������ ����
        // ���� ������ ����ִ� ���
        if (this.grabbed_block == null)
        {
            if(!this.IsAnyFallingBlock())
            {
                // ���콺 ��ư�� ����
                if (Input.GetMouseButtonDown(0))
                {
                    foreach(BlockControl block in this.blocks)
                    {
                        // ������ ���� �� ���� ���
                        if (!block.IsGrabbable())
                        {
                            continue;   // ������ ó������ ����
                        }

                        // ���콺 ��ġ�� ���� ���� ���̸�
                        if (!block.IsContainedPosition(mouse_position_xy))
                        {
                            continue;   // ������ ó������ ����
                        }

                        // ó�� ���� ������ grabbed_block�� ���
                        this.grabbed_block = block;

                        // ����� ���� ó���� ����
                        this.grabbed_block.BeginGrab();
                        break;
                    }
                }
            }
        }
        // ������ ����� ��
        else
        {
            do
            {
                // �����̵��� ���� ������ ������
                BlockControl swap_target = this.GetNextBlock(grabbed_block, grabbed_block.slide_dir);

                // �����̵��� ���� ������ ���������
                if (swap_target==null)
                {
                    break;  // ���� Ż��
                }
                // �����̵��� ���� ������ ���� �� ���� ���¸�
                if (!swap_target.IsGrabbable())
                {
                    break;  // ���� Ż��
                }
                
                // ���� ��ġ���� �����̵� ��ġ������ �Ÿ��� ����
                float offset = this.grabbed_block.CalcDirOffset(mouse_position_xy, this.grabbed_block.slide_dir);

                // ���� �Ÿ��� ���� ũ���� ���ݺ��� �۴ٸ�
                if (offset < Block.COLLISION_SIZE / 2.0f)
                {
                    break;  // ���� Ż��
                }

                this.SwapBlock(grabbed_block, grabbed_block.slide_dir, swap_target);
                this.grabbed_block = null;  // ������ ���� ����
            } while (false);

            // ���콺 ��ư�� ���� ���� ������
            if (!Input.GetMouseButton(0))
            {
                //������ ������ ���� ó���� ����
                this.grabbed_block.EndGrab();

                // grabbed_block�� ���
                this.grabbed_block = null;
            }

            // ���� �� �Ǵ� �����̵� ���̸�
            if (this.IsAnyFallingBlock() || this.IsAnySlidingBlock())
            {
                // �ƹ��͵� ���� �ʴ´�.
            }
            else
            {
                int ignite_count = 0;   // ���� ���� ����

                // �׷��� �� ��� ���Ͽ� ���� ó��
                foreach(BlockControl block in this.blocks)
                {
                    // �������
                    if(!block.IsIdle())
                    {
                        continue;   // ���� �� ���� ���� ó��
                    }

                    // ���� �Ǵ� ���ο� ���� �� ������ �� �� �̻� �����Ǿ��ִٸ�
                    if (this.CheckConnection(block))
                    {
                        ignite_count++; // ���� ���� ���� ����
                    }
                }


                // ���� ���� ������ 0���� ũ��(�� ������ ������ ���� ������)
                if (ignite_count > 0)
                {
                    // ���� ��ȭ�� �ƴ϶��, ��ȭ Ƚ�� ����
                    if (!this.is_vanishing_prev)
                    {
                        this.score_counter.ClearIgniteCount();
                    }

                    // ��ȭ Ƚ�� ����
                    this.score_counter.AddIgniteCount(ignite_count);
                    // �հ� ���� ����
                    this.score_counter.UpdateTotalScore();
                    
                    // �� ������ ������ ���� ������ ����
                    int block_count = 0;    // ���� �ٴ� ���� ������ ����

                    // �׸��� ���� ��� ���Ͽ� ���� ó��
                    foreach (BlockControl block in this.blocks)
                    {
                        // Ÿ�� ���̸�
                        if(block.IsVanishing())
                        {
                            block.RewindVanishTimer();  // ����ȭ
                            block_count++;
                        }
                    }
                }
            }

        }

        // �ϳ��� ���� ���� ������ �ִ��� Ȯ��
        bool is_vanishing = this.IsAnyVanishingBlock();

        // ������ �����Ǹ� ������ ����߸�
        do
        {
            // ���� ���� ������ �ִٸ�
            if(is_vanishing)
            {
                break;  // �������� ����
            }
            // ��ü ���� ������ �ִٸ�
            if (this.IsAnySlidingBlock())
            {
                break;  // �������� ����
            }

            for(int x = 0; x < Block.BLOCK_NUM_X; x++)
            {
                // ���� ��ü ���� ������ �ִٸ� �� ���� ó������ �ʰ� ���� ���� ����
                if (this.IsSlidingBlockInColumn(x))
                {
                    continue;
                }

                // �� ���� �ִ� ������ ���������� �˻�
                for (int y = 0; y <Block.BLOCK_NUM_Y-1; y++)
                {
                    // ���� ������ ��ǥ�ö�� ���� ��������
                    if (!this.blocks[x,y].IsVacant())
                    {
                        continue;
                    }

                    // ���� ���� �Ʒ��� �ִ� ������ �˻�
                    for(int y1=y+1; y1 <Block.BLOCK_NUM_Y; y1++)
                    {
                        // �Ʒ��� �ִ� ������ ��ǥ�ø� ���� �������� �Ѿ
                        if(this.blocks[x,y1].IsVacant())
                        {
                            continue;
                        }

                        // ���� ��ü
                        this.FallBlock(this.blocks[x, y], Block.DIR4.UP, this.blocks[x, y1]);
                        break;
                    }
                }
            }

            for(int x = 0; x < Block.BLOCK_NUM_X; x++)
            {
                int fall_start_y = Block.BLOCK_NUM_Y;
                for(int y = 0; y < Block.BLOCK_NUM_Y; y++)
                {
                    // ��ǥ�� ������ �ƴ϶�� ���� ��������
                    if (!this.blocks[x,y].IsVacant())
                    {
                        continue;
                    }

                    this.blocks[x, y].BeginRespawn(fall_start_y);   // ���� �����
                    fall_start_y++;
                }
            }

        } while (false);

        this.is_vanishing_prev = is_vanishing;
    }

    public void InitialSetUp()
    {
        // �׸����� ũ��: 9x9
        this.blocks = new BlockControl[Block.BLOCK_NUM_X, Block.BLOCK_NUM_Y];
        // ������ �� ��ȣ
        int color_index = 0;
        Block.COLOR color = Block.COLOR.FIRST;

        for(int y =0; y<Block.BLOCK_NUM_Y; y++)         // ó������ ������ �����
        {
            for (int x = 0; x < Block.BLOCK_NUM_X; x++)    // ���ʺ��� �����ʱ���
            {
                // BlockPrefab�� �ν��Ͻ��� ���� ����
                GameObject game_object = Instantiate(this.BlockPrefab) as GameObject;
                // ������ ������ BlockControl Ŭ���� ��������
                BlockControl block = game_object.GetComponent<BlockControl>();
                // ������ �׸��忡 ����
                this.blocks[x, y] = block;

                // ������ ��ġ ����(�׸��� ��ǥ) ����
                block.pos.x = x;
                block.pos.y = y;
                // �� BlockControl�� ������ GameRoot�� �ڽ�
                block.block_root = this;

                // �׸��� ��ǥ�� ���� ��ġ(�� ��ǥ)�� ��ȯ
                Vector3 position = BlockRoot.CalcBlockPosition(block.pos);
                // ���� ���� ��ġ �̵�
                block.transform.position = position;
                //// ������ �� ����
                //block.SetColor((Block.COLOR)color_index);
                // ���� ���� Ȯ���� �������� �� ����
                color = this.SelectBlockColor();
                block.SetColor(color);
                // ������ �̸��� ���� (���� ���� ���� Ȯ�ο� �ʿ�)
                block.name = "block(" + block.pos.x.ToString() + "," + block.pos.y.ToString() + ")";
                // ��ü �� �� �ϳ��� ���� ���Ƿ� ����
                color_index = Random.Range(0, (int)Block.COLOR.NORMAL_COLOR_NUM);
            }
        }
    }

    // ������ �׸��� ��ǥ�� �������� ��ǥ ���
    public static Vector3 CalcBlockPosition(Block.Position pos)
    {
        // ��ġ�� ���� �� ���� ��ġ�� �ʱⰪ���� ����
        Vector3 position = new Vector3(-(Block.BLOCK_NUM_X / 2.0f - 0.5f), -(Block.BLOCK_NUM_Y / 2.0f - 0.5f), 0.0f);

        // �ʱ갪 + �׸��� ��ǥ x ���� ũ��
        position.x += (float)pos.x * Block.COLLISION_SIZE;
        position.y += (float)pos.y * Block.COLLISION_SIZE;

        return position;          //�������� ��ǥ ��ȯ
    }

    public bool UnprojectMousePosition(out Vector3 world_position, Vector3 mouse_position)
    {
        bool result;

        // ī�޶� ���� �ڸ� ���ϴ� �� �ۼ�(Vecotr3.back)
        // ������ ���� ũ�⸸ŭ �տ� ��
        Plane plane = new Plane(Vector3.back, new Vector3(0.0f, 0.0f, -Block.COLLISION_SIZE / 2.0f));

        // ī�޶�� ���콺 ����ϴ� ���� ����
        Ray ray = this.main_camera.GetComponent<Camera>().ScreenPointToRay(mouse_position);

        float depth;

        // ����(ray)�� ��(plane)�� ��Ҵٸ�
        if (plane.Raycast(ray,out depth))       // depth�� ���� ���
        {
            // �μ� world_position�� ���콺 ��ġ�� �����
            world_position = ray.origin + ray.direction * depth;
            result = true;
        }
        // ���� �ʾҴٸ�
        else
        {
            // �μ� world_position�� 0�� ���ͷ� �����
            world_position = Vector3.zero;
            result = false;
        }

        return result;
    }

    public BlockControl GetNextBlock(BlockControl block, Block.DIR4 dir)
    {
        // �����̵��� ���� ������ ���⿡ ����
        BlockControl next_block = null;

        switch (dir)
        {
            case Block.DIR4.RIGHT:
                // �׷��� �� ����
                if (block.pos.x < Block.BLOCK_NUM_X - 1)
                {
                    next_block = this.blocks[block.pos.x + 1, block.pos.y];
                }
                break;
            case Block.DIR4.LEFT:
                // �׷��� �� ����
                if (block.pos.x > 0)
                {
                    next_block = this.blocks[block.pos.x - 1, block.pos.y];
                }
                break;
            case Block.DIR4.UP:
                // �׷��� �� ����
                if (block.pos.x > 0)
                {
                    next_block = this.blocks[block.pos.x, block.pos.y + 1];
                }
                break;
            case Block.DIR4.DOWN:
                // �׷��� �� ����
                if (block.pos.x > 0)
                {
                    next_block = this.blocks[block.pos.x, block.pos.y - 1];
                }
                break;
        }

        return next_block;
    }

    public static Vector3 GetDirVector(Block.DIR4 dir)
    {
        Vector3 vec = Vector3.zero;

        switch (dir)
        {
            case Block.DIR4.RIGHT:
                vec = Vector3.right;    // ������ 1ĭ �̵�
                break;
            case Block.DIR4.LEFT:
                vec = Vector3.left;     // ���� 1ĭ �̵�
                break;
            case Block.DIR4.UP:
                vec = Vector3.up;       // ���� 1ĭ �̵�
                break;
            case Block.DIR4.DOWN:
                vec = Vector3.down;     // �Ʒ��� 1ĭ �̵�
                break;
            case Block.DIR4.NUM:
                break;
        }

        vec *= Block.COLLISION_SIZE;    // ������ ũ��� ����

        return vec;
    }

    public static Block.DIR4 GetOppositDir(Block.DIR4 dir)
    {
        Block.DIR4 opposit = dir;

        switch (dir)
        {
            case Block.DIR4.RIGHT:
                opposit = Block.DIR4.LEFT;
                break;
            case Block.DIR4.LEFT:
                opposit = Block.DIR4.RIGHT;
                break;
            case Block.DIR4.UP:
                opposit = Block.DIR4.DOWN;
                break;
            case Block.DIR4.DOWN:
                opposit = Block.DIR4.UP;
                break;
        }

        return opposit;
    }

    // ������ ��ü�ϴ� �Լ�
    public void SwapBlock(BlockControl block0, Block.DIR4 dir, BlockControl block1)
    {
        // ������ ���� ���� ���
        Block.COLOR color0 = block0.color;
        Block.COLOR color1 = block1.color;

        // ������ Ȯ������ ���� ���
        Vector3 scale0 = block0.transform.localScale;
        Vector3 scale1 = block1.transform.localScale;

        // ������ ������� �ð��� ���� ���
        float vanish_timer0 = block0.vanish_timer;
        float vanish_timer1 = block1.vanish_timer;

        // ������ �̵��� ��ġ�� ���� ����
        Vector3 offset0 = BlockRoot.GetDirVector(dir);
        Vector3 offset1 = BlockRoot.GetDirVector(BlockRoot.GetOppositDir(dir));

        // �� ��ü
        block0.SetColor(color1);
        block1.SetColor(color0);

        // Ȯ���� ��ü
        block0.transform.localScale = scale1;
        block1.transform.localScale = scale0;

        // ������� �ð��� ��ü
        block0.vanish_timer = vanish_timer1;
        block1.vanish_timer = vanish_timer0;
        block0.BeginSlide(offset0); // ���� ���� �̵� ����
        block1.BeginSlide(offset1); // �̵��� ��ġ�� ���� �̵� ����
    }

    // �μ��� ���� ������ �� ���� ���� �ȿ� ������ �ľ��ϴ� �޼���
    public bool CheckConnection(BlockControl start)
    {
        bool result = false;
        int normal_block_num = 0;

        // �μ��� ������ �Һ��� ������ �ƴϸ�
        if (!start.IsVanishing())
        {
            normal_block_num = 1;
        }

        // �׸��� ��ǥ�� ���
        int rx = start.pos.x;
        int lx = start.pos.x;


        // ������ �� �� �˻�
        for(int x = lx - 1; x > 0; x--)
        {
            BlockControl next_block = this.blocks[x, start.pos.y];

            // ���� �ٸ���
            if (next_block.color != start.color)
            {
                break;  // ������ ��������
            }

            // ���� ���̸�
            if(next_block.step == Block.STEP.FALL || next_block.next_step == Block.STEP.FALL)
            {
                break;  // ������ ��������
            }

            // �����̵� ���̸�
            if (next_block.step == Block.STEP.SLIDE || next_block.next_step == Block.STEP.SLIDE)
            {
                break;  // ������ ��������
            }

            // �Һ��� ���°� �ƴϸ�
            if (!next_block.IsVanishing())
            {
                normal_block_num++; // �˻�� ī���͸� ����
            }

            lx = x;
        }

        // ������ �������� �˻�
        for(int x = rx+1; x<Block.BLOCK_NUM_X; x++)
        {
            BlockControl next_block = this.blocks[x, start.pos.y];

            // ���� �ٸ���
            if (next_block.color != start.color)
            {
                break;
            }

            // ���� ���̸�
            if (next_block.step == Block.STEP.FALL || next_block.next_step == Block.STEP.FALL)
            {
                break;  // ������ ��������
            }

            // �����̵� ���̸�
            if (next_block.step == Block.STEP.SLIDE || next_block.next_step == Block.STEP.SLIDE)
            {
                break;  // ������ ��������
            }

            // �Һ��� ���°� �ƴϸ�
            if (!next_block.IsVanishing())
            {
                normal_block_num++; // �˻�� ī���͸� ����
            }

            rx = x;
        }

        do
        {
            // ������ ������ �׸��� ��ȣ -
            // ���� ������ �׸��� ��ȣ +
            // �߾� ����(1)�� ���� ���� 3 �̸��̸�
            if (rx - lx + 1 < 3)
            {
                break;
            }

            // �Һ��� ���� ������ �ϳ��� ������
            if(normal_block_num==0)
            {
                break;  // ������ ��������
            }

            for (int x = lx; x < rx + 1; x++)
            {
                this.blocks[x, start.pos.y].ToVanishing();
                result = true;
            }
        } while (false);

        normal_block_num = 0;
        if(!start.IsVanishing())
        {
            normal_block_num = 1;
        }

        int uy = start.pos.y;
        int dy = start.pos.y;

        // ���� ������ �˻�
        for (int y = dy - 1; y > 0; y--)
        {
            BlockControl next_block = this.blocks[start.pos.x, y];

            // ���� �ٸ���
            if (next_block.color != start.color)
            {
                break;
            }

            // ���� ���̸�
            if (next_block.step == Block.STEP.FALL || next_block.next_step == Block.STEP.FALL)
            {
                break;  // ������ ��������
            }

            // �����̵� ���̸�
            if (next_block.step == Block.STEP.SLIDE || next_block.next_step == Block.STEP.SLIDE)
            {
                break;  // ������ ��������
            }

            // �Һ��� ���°� �ƴϸ�
            if (!next_block.IsVanishing())
            {
                normal_block_num++; // �˻�� ī���͸� ����
            }

            dy = y;
        }

        // ������ �Ʒ��� �˻�
        for (int y = uy + 1; y < Block.BLOCK_NUM_Y; y++)
        {
            BlockControl next_block = this.blocks[start.pos.x, y];

            // ���� �ٸ���
            if (next_block.color != start.color)
            {
                break;
            }

            // ���� ���̸�
            if (next_block.step == Block.STEP.FALL || next_block.next_step == Block.STEP.FALL)
            {
                break;  // ������ ��������
            }

            // �����̵� ���̸�
            if (next_block.step == Block.STEP.SLIDE || next_block.next_step == Block.STEP.SLIDE)
            {
                break;  // ������ ��������
            }

            // �Һ��� ���°� �ƴϸ�
            if (!next_block.IsVanishing())
            {
                normal_block_num++; // �˻�� ī���͸� ����
            }

            uy = y;
        }

        do
        {
            if(uy - dy + 1 < 3)
            {
                break;
            }

            if(normal_block_num == 0)
            {
                break;
            }

            for (int y = dy; y < uy + 1; y++) 
            {
                this.blocks[start.pos.x, y].ToVanishing();
                result = true;
            }
        } while (false);

        return result;
    }

    // �Һٴ� ���� ������ �ϳ��� ������ true�� ��ȯ
    private bool IsAnyVanishingBlock()
    {
        bool result = false;
        foreach (BlockControl block in this.blocks)
        {
            if(block.vanish_timer > 0.0f)
            {
                result = true;
                break;
            }
        }

        return result;
    }

    // �����̵� ���� ������ �ϳ��� ������ true�� ��ȯ
    private bool IsAnySlidingBlock()
    {
        bool result = false;
        foreach (BlockControl block in this.blocks)
        {
            if(block.step == Block.STEP.SLIDE)
            {
                result = true;
                break;
            }
        }

        return result;
    }

    // ���� ���� ������ �ϳ��� ������ true�� ��ȯ
    private bool IsAnyFallingBlock()
    {
        bool result = false;
        foreach (BlockControl block in this.blocks)
        {
            if(block.step == Block.STEP.FALL)
            {
                result = true;
                break;
            }
        }

        return result;
    }

    // ���� �� ���Ʒ� ���� ��ü
    public void FallBlock(BlockControl block0, Block.DIR4 dir, BlockControl block1)
    {
        // block0�� block1�� ��, ũ��, ����� ������ �ɸ��� �ð�, ǥ��, ��ǥ��, ���� ���
        Block.COLOR color0 = block0.color;
        Block.COLOR color1 = block1.color;
        Vector3 scale0 = block0.transform.localScale;
        Vector3 scale1 = block1.transform.localScale;
        float vanish_timer0 = block0.vanish_timer;
        float vanish_timer1 = block1.vanish_timer;
        bool visible0 = block0.IsVisible();
        bool visible1 = block1.IsVisible();
        Block.STEP step0 = block0.step;
        Block.STEP step1 = block1.step;

        // block0�� block1�� ���� �Ӽ� ��ü
        block0.SetColor(color1);
        block1.SetColor(color0);
        block0.transform.localScale = scale1;
        block1.transform.localScale = scale0;
        block0.vanish_timer = vanish_timer1;
        block1.vanish_timer = vanish_timer0;
        block0.SetVisible(visible1);
        block1.SetVisible(visible0);
        block0.step = step1;
        block1.step = step0;
        block0.BeginFall(block1);
    }

    // ���� �׸��� ��ǥ�� ���� �����̵� ���� ������ �ִ��� Ȯ���ϴ� �Լ�
    private bool IsSlidingBlockInColumn(int x)
    {
        bool result = false;
        for(int y = 0; y < Block.BLOCK_NUM_Y; y++)
        {
            // �����̵� ���� ������ ������
            if(this.blocks[x,y].IsSliding())
            {
                result = true;
                break;
            }
        }

        return result;
    }

    public void Create()
    {
        this.level_control = new LevelControl();
        this.level_control.Initialize();                    // ���� ������ �ʱ�ȭ
        this.level_control.LoadLevelData(this.levelData);   // ������ �б�
        this.level_control.SelectLevel();                   // ���� ����
    }

    public Block.COLOR SelectBlockColor()
    {
        Block.COLOR color = Block.COLOR.FIRST;
        // �̹� ������ ���� �����͸� ������
        LevelData level_data = this.level_control.GetCurrentLevelData();
        float rand = Random.Range(0.0f, 1.0f);  // 0.0~1.0 ������ ����
        float sum = 0.0f;
        int i = 0;

        // ������ ���� ��ü�� ó���ϴ� ����
        for(i=0; i<level_data.probability.Length-1; i++)
        {
            if(level_data.probability[i] == 0.0f)
            {
                continue;   // ���� Ȯ���� 0�̸� ������ ó������ ����
            }
            sum += level_data.probability[i];   // ���� Ȯ���� ����

            // �հ谡 �������� ������
            if (rand<sum)
            {
                break;  // ������ ��������
            }
        }

        color = (Block.COLOR)i;     // i��° �� ��ȯ
        return color;
    }
}