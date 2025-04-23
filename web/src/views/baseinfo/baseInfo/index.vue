<template>
  <a-card :bordered="false">
    <div class="table-header">
      <span class="title">基本资料</span>
    </div>
    <a-form :model="formState" layout="horizontal" :label-col="{ style: { width: '100px' } }">
      <!-- 顶部表单字段 -->
      <a-row :gutter="16" style="margin-bottom: 16px">
        <a-col :span="6">
          <a-form-item label="任务类型" style="margin-bottom: 0">
            <a-radio-group v-model:value="formState.taskType" disabled>
              <a-radio value="0">月度</a-radio>
              <a-radio value="1">年度</a-radio>
            </a-radio-group>
          </a-form-item>
        </a-col>
        <a-col :span="6">
          <a-form-item label="任务编码" style="margin-bottom: 0">
            <a-input
              v-model:value="formState.taskCode"
              placeholder="请输入版次"
              :bordered="false"
              readonly
            />
          </a-form-item>
        </a-col>
        <a-col :span="6">
          <a-form-item label="版次" style="margin-bottom: 0">
            <a-input
              v-model:value="formState.version"
              :bordered="false"
              placeholder="请输入版次"
              readonly
            />
          </a-form-item>
        </a-col>
        <a-col :span="6">
          <a-form-item label="操作人员" style="margin-bottom: 0" disabled>
            <a-input
              v-model:value="formState.operator"
              :bordered="false"
              placeholder="请输入操作人员"
              readonly
            />
          </a-form-item>
        </a-col>
        <a-col v-if="showCopyFrom" :span="6">
          <a-form-item label="Copy From" style="margin-bottom: 0">
            <a-select
              v-model:value="formState.copyFrom"
              placeholder="请选择Copy From"
              :options="copyFromOptions"
              style="width: 100%"
              @change="handleCopyFromChange"
            />
          </a-form-item>
        </a-col>
      </a-row>

      <!-- 表格区域 -->
      <a-form-item>
        <DynamicTable
          row-key="id"
          :data-request="loadData"
          :data-source="dataSource"
          :columns="columns"
          bordered
          size="small"
        >
          <template #bodyCell="{ column, record, index }">
            <template v-if="column.edittype === 'operation'">
              <a-button type="link" @click="handleEdit(record)">
                {{ record.editable ? '保存' : '编辑' }}
              </a-button>
              <a-button
                v-if="record.editable"
                type="link"
                style="margin-left: 8px"
                @click="handleCancel(record)"
              >
                取消
              </a-button>
              <a-button
                v-if="!record.editable"
                type="link"
                style="margin-left: 8px; color: red"
                @click="handleDelete(record)"
              >
                删除
              </a-button>
            </template>
            <template v-else-if="column.edittype === 'select' && record.editable">
              <a-select
                v-model:value="record.preparationType"
                show-search
                :filter-option="filterOption"
                :options="column.formItemProps.componentProps.options"
                style="width: 100%"
              />
            </template>
            <template v-else-if="column.edittype === 'select2' && record.editable">
              <a-select
                v-model:value="record.groupName"
                show-search
                :options="column.formItemProps.componentProps.options"
                style="width: 100%"
              />
            </template>
            <template v-else-if="column.edittype === 'input' && record.editable">
              <a-input v-model:value="record[column.key]" style="margin: -5px 0" />
            </template>

            <template v-else>
              <template v-if="column.dataIndex === 'preparationType'">
                {{ record.preparationType === 0 ? '天数' : '安库' }}
              </template>
              <template v-else-if="column.key == 'INDEX'">
                {{ index }}
              </template>
              <template v-else>{{ record[column.key] }}</template>
            </template>
          </template>
          <template #afterHeaderTitle>
            <div class="flex gap-2 ml-2" style="font-size: 16px; font-weight: 500">对接窗口</div>
          </template>
          <template #toolbar>
            <a-button type="primary" @click="handleAdd">
              {{ $t('common.add') }}
            </a-button>
          </template>
        </DynamicTable>
      </a-form-item>

      <!-- 物料群组表格 -->
      <!-- <a-form-item>
        <div class="table-header">
          <span class="title">物料群组收集范围（请填写群组前N码）</span>
          <a-button type="primary" @click="handleAddGroup"> <plus-outlined /> 新增 </a-button>
        </div>
        <a-table
          :columns="groupColumns"
          :data-source="groupData"
          :pagination="false"
          bordered
          size="middle"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.dataIndex === 'operation'">
              <a-space>
                <a @click="handleEditGroup(record)">编辑</a>
                <a-popconfirm title="确定要删除这条记录吗？" @confirm="handleDeleteGroup(record)">
                  <a class="text-red-500">删除</a>
                </a-popconfirm>
              </a-space>
            </template>
          </template>
        </a-table>
      </a-form-item> -->

      <!-- 底部按钮 -->
      <a-form-item :wrapper-col="{ span: 24 }" style="text-align: center">
        <a-space :size="24">
          <a-button type="primary" @click="handleSave">保存</a-button>
          <a-button @click="handleExecute">执行</a-button>
        </a-space>
      </a-form-item>
    </a-form>
  </a-card>
</template>

<script lang="tsx" setup>
  import { ref, reactive, computed, onMounted, watch } from 'vue'
  import { Alert } from 'ant-design-vue'
  import { useTable, useTablePlugin, type TableColumn } from '@/components/core/dynamic-table'
  import { useI18n } from '@/hooks/useI18n'
  import { baseColumns } from './columns'
  import { formSchemas } from './formSchemas'
  import action from '@/api/baseinfo/baseInfo'

  defineOptions({
    name: 'BaseInfo',
  })

  const dataSource = ref<any[]>([])
  // 表单状态
  const formState = reactive({
    taskType: '0',
    taskCode: '',
    version: '',
    operator: '',
    copyFrom: '',
    deptCode: '',
    courseName: '',
    contact: '',
    id: '',
  })

  const showCopyFrom = ref(true)
  const copyFromOptions = ref<API.SelectResult[]>([])
  const handleCopyFromChange = async (value: string) => {
    if (value) {
      // const res = await forecastIrregularityApi.getDocksAndGroupsByVersion(value)
      // tableData.value = res.dockWindowList
      // groupData.value = res.bomGroupList
    } else {
      // tableData.value = []
      // groupData.value = []
    }
  }

  const { t } = useI18n()
  const [DynamicTable, dynamicTableInstance] = useTable({
    pagination: false,
    size: 'small',
    rowKey: 'id',
    bordered: true,
    search: false,
    autoHeight: true,
  })
  const {
    rowSelection,
    checkRows,
    exportLoading,
    loadData,
    aoaToExcel,
    handleCancelSelect,
    openModal,
    delRowConfirm,
  } = useTablePlugin({ dynamicTableInstance, action, columns: baseColumns, formSchemas })

  /**
   * 表格列
   */
  // 表格列
  const columns: ref<TableColumn<any>[]> = [
    {
      title: 'id',
      dataIndex: 'id',
      width: 55,
      align: 'center',
      hideInTable: true,
      hideInSearch: true,
    },
    {
      title: '部门编码',
      width: 200,
      align: 'center',
      edittype: 'select2',
      dataIndex: 'deptCode',
      formItemProps: {
        componentProps: {
          options: [],
        },
      },
    },
    {
      title: '课别名称',
      dataIndex: 'courseName',
      align: 'center',
      edittype: 'select',
      width: 150,
      formItemProps: {
        component: 'Select',
        componentProps: {
          options: [
            {
              label: '天数',
              value: 0,
            },
            {
              label: '安库',
              value: 1,
            },
          ],
        },
      },
    },
    {
      title: '联系人',
      width: 200,
      align: 'center',
      edittype: 'input',
      dataIndex: 'contact',
    },
    {
      title: '说明',
      width: 200,
      align: 'center',
      dataIndex: 'description',
    },
    {
      title: '操作',
      width: 150,
      dataIndex: 'operation',
      edittype: 'operation',
      hideInSearch: true,
      align: 'center',
      fixed: 'right',
    },
  ]

  const handleAdd = () => {
    const newRow = {
      id: '',
      deptCode: '',
      courseName: '',
      contact: '',
      editable: true, // 新增行直接进入编辑模式
    }
    dataSource.value.unshift(newRow) // 添加到表格顶部
  }
</script>
<style scoped>
  .table-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 16px;
    padding: 8px;
    background-color: #f5f5f5;
  }

  .table-header .title {
    font-size: 16px;
    font-weight: 500;
  }

  .ant-card {
    margin: 16px;
  }

  ::v-deep .row-error {
    background-color: #e5b7b7 !important; /* 浅红色 */
  }

  ::v-deep .ant-table-row .row-error:hover {
    background-color: #e39d9d !important; /* 浅红色 */
  }

  ::v-deep .p-16px {
    margin-bottom: 10px;
    padding: 8px;
    background-color: #f5f5f5 !important; /* 浅红色 */
  }
</style>
