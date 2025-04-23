<template>
  <div>
    <DynamicTable
      ref="dynamicTableRef"
      row-key="id"
      :header-title="$t('routes.menu.sys.role')"
      :data-request="action.getListByPage"
      :columns="columns"
      bordered
      size="small"
    >
      <template #bodyCell="{ column, record }">
        <template v-if="column.dataIndex === 'isDeleted'">
          <SearchOutlined />
        </template>
      </template>
      <template #toolbar>
        <a-button v-if="$auth('/api/SysRole/Add')" type="primary" @click="openMenuModal({})">
          {{ $t('common.add') }}
        </a-button>
      </template>
    </DynamicTable>
  </div>
</template>

<script lang="ts" setup>
  import { ref, watch, nextTick, onMounted } from 'vue'
  import { useRoute, useRouter } from 'vue-router'
  import type { TreeDataItem } from 'ant-design-vue/lib/tree/Tree'
  import { DynamicTable } from '@/components/core/dynamic-table'
  import { useFormModal } from '@/hooks/useModal/useFormModal'
  import type { FormSchema } from '@/components/core/schema-form/src/types/form'
  import { formatDept2Tree, formatMenu2Tree } from '@/utils/tree'
  import type { TableColumn } from '@/components/core/dynamic-table'
  import action from '@/api/sys/role'
  import menuAction from '@/api/sys/menu'
  import { useI18n } from '@/hooks/useI18n'
  import { SearchOutlined } from '@ant-design/icons-vue'
  import type { SelectProps } from 'ant-design-vue'

  defineOptions({
    name: 'SysRole',
  })

  interface KVITEM {
    label?: any
    value?: string | number | null
  }

  const types = ref<KVITEM[]>([
    { value: '1', label: '类型1' },
    { value: '2', label: '类型2' },
  ])

  const options1 = ref<SelectProps['options']>([
    {
      value: 'jack',
      label: 'Jack',
    },
    {
      value: 'lucy',
      label: 'Lucy',
    },
    {
      value: 'disabled',
      label: 'Disabled',
      disabled: true,
    },
    {
      value: 'yiminghe',
      label: 'Yiminghe',
    },
  ])

  const statusMap = ref<SelectProps['options']>([])

  const dynamicTableRef = ref<InstanceType<typeof DynamicTable>>()
  const { t } = useI18n()

  const [showModal] = useFormModal()
  const route = useRoute()
  const router = useRouter()

  const getCheckedKeys = (checkedList: number[], options: TreeDataItem[], total = []) => {
    return options.reduce<number[]>((prev, curr) => {
      if (curr.children?.length) {
        getCheckedKeys(checkedList, curr.children, total)
      } else {
        if (checkedList.includes(curr.value)) {
          prev.push(curr.value)
        }
      }
      return prev
    }, total)
  }

  /**
   * @description 打开新增/编辑弹窗
   */
  const openMenuModal = async record => {
    const [formRef] = await showModal({
      modalProps: {
        title: `${record.id ? t('common.edit') : t('common.add')}${t('column.role')}`,
        width: '50%',
        onFinish: async values => {
          values.id = record.id
          const menusRef = formRef?.compRefMap.get('menuIds')!
          const params = {
            ...values,
            menus: [...menusRef.halfCheckedKeys, ...menusRef.checkedKeys],
          }
          console.log('新增/编辑角色', params)
          await (record.id ? action.update : action.add)(params)
          dynamicTableRef.value?.reload()
        },
      },
      formProps: {
        labelWidth: 100,
        schemas: roleSchemas,
      },
    })

    const menuData = await menuAction.getMenuList()
    const menuTree = formatMenu2Tree(menuData)

    formRef?.updateSchema([
      {
        field: 'menus',
        componentProps: { treeData: menuTree },
      },
    ])
    if (record.id) {
      const data = await action.getById(record.id)
      const menus = await menuAction.getMenuListByRoleId(record.id)
      const menuIds = menus.map(n => n.id)

      formRef?.setFieldsValue({
        ...record,
        roleName: data.roleName,
        description: data.description,
        menus: getCheckedKeys(menuIds, menuTree),
      })
    }
  }

  const delRowConfirm = async record => {
    await action.delete(record.id)
    dynamicTableRef.value?.reload()
  }

  // 模拟角色类型数据
  const roleTypes = [
    { value: '1', label: '类型1' },
    { value: '2', label: '类型2' },
  ]

  // 存储当前选择的角色类型
  const selectedRoleType = ref('')

  // 存储角色状态数据
  const roleStatusMap = ref([
    { value: '11', label: '状态1', roleType: '1' },
    { value: '22', label: '状态2', roleType: '1' },
    { value: '33', label: '状态3', roleType: '2' },
    { value: '44', label: '状态4', roleType: '2' },
  ])

  const columns = ref<TableColumn<any>[]>([
    {
      title: 'id',
      dataIndex: 'id',
      width: 55,
      align: 'center',
      hideInTable: true,
      hideInSearch: true,
    },
    {
      title: '角色类型',
      dataIndex: 'roleType',
      width: 200,
      align: 'center',
      formItemProps: {
        component: 'Select',
        componentProps: ({ formInstance }) => ({
          showSearch: true, //打开下拉搜索
          optionFilterProp: 'label', // label 作为搜索条件
          options: types.value,
          onChange: (value: string) => {
            formInstance.updateSchema({
              field: 'roleStatus',
              componentProps: {
                options: roleStatusMap.value.filter(status => status.roleType === value),
              },
            })

            formInstance.setFieldsValue({
              roleStatus: undefined, // 或者 null
            })
          },
        }),
      },
    },
    {
      title: '角色状态',
      dataIndex: 'roleStatus',
      width: 200,
      align: 'center',
      formItemProps: {
        component: 'Select',
        componentProps: {
          options: [],
          // defaultValue: '',
        },
      },
    },
    {
      title: '名称',
      width: 200,
      align: 'center',
      dataIndex: 'roleName',
    },
    {
      title: '描述',
      dataIndex: 'description',
      align: 'center',
      hideInSearch: true,
    },
    {
      title: '创建人',
      dataIndex: 'createUser',
      align: 'center',
    },
    {
      title: '创建时间',
      dataIndex: 'createTime',
      align: 'center',
      hideInSearch: true,
    },
    {
      title: '更新时间',
      align: 'center',
      dataIndex: 'updateTime',
      hideInSearch: true,
    },
    // {
    //   title: '操作',
    //   width: 300,
    //   dataIndex: 'ACTION',
    //   hideInSearch: true,
    //   align: 'center',
    //   fixed: 'right',
    //   actions: ({ record }) => [
    //     {
    //       label: t('common.edit'),
    //       onClick: () => openMenuModal(record),
    //     },
    //     {
    //       label: t('common.delete'),
    //       danger: true,
    //       popConfirm: {
    //         title: t('column.confirmDel'),
    //         onConfirm: () => delRowConfirm(record),
    //       },
    //     },
    //   ],
    // },
  ])

  // 表格列
  // const columns: TableColumn<any>[] = [
  //   {
  //     title: 'id',
  //     dataIndex: 'id',
  //     width: 55,
  //     align: 'center',
  //     hideInTable: true,
  //     hideInSearch: true,
  //   },
  //   {
  //     title: '角色类型',
  //     dataIndex: 'roleType',
  //     width: 200,
  //     align: 'center',
  //     formItemProps: {
  //       component: 'Select',
  //       componentProps: {
  //         options: roleTypes,
  //         onChange: handleRoleTypeChange,
  //       },
  //     },
  //   },
  //   {
  //     title: '角色状态',
  //     dataIndex: 'roleStatus',
  //     width: 200,
  //     align: 'center',
  //     formItemProps: {
  //       component: 'Select',
  //       componentProps: {
  //         options: filteredRoleStatusMap.value,
  //         vModel: 'roleStatus',
  //       },
  //     },
  //   },
  //   {
  //     title: '名称',
  //     width: 200,
  //     align: 'center',
  //     dataIndex: 'roleName',
  //   },
  //   {
  //     title: '描述',
  //     dataIndex: 'description',
  //     align: 'center',
  //     hideInSearch: true,
  //   },
  //   {
  //     title: '创建人',
  //     dataIndex: 'createUser',
  //     align: 'center',
  //   },
  //   {
  //     title: '创建时间',
  //     dataIndex: 'createTime',
  //     align: 'center',
  //     hideInSearch: true,
  //   },
  //   {
  //     title: '更新时间',
  //     align: 'center',
  //     dataIndex: 'updateTime',
  //     hideInSearch: true,
  //   },
  //   // {
  //   //   title: '操作',
  //   //   width: 300,
  //   //   dataIndex: 'ACTION',
  //   //   hideInSearch: true,
  //   //   align: 'center',
  //   //   fixed: 'right',
  //   //   actions: ({ record }) => [
  //   //     {
  //   //       label: t('common.edit'),
  //   //       onClick: () => openMenuModal(record),
  //   //     },
  //   //     {
  //   //       label: t('common.delete'),
  //   //       danger: true,
  //   //       popConfirm: {
  //   //         title: t('column.confirmDel'),
  //   //         onConfirm: () => delRowConfirm(record),
  //   //       },
  //   //     },
  //   //   ],
  //   // },
  // ]

  // 表单信息
  const roleSchemas: FormSchema[] = [
    {
      field: 'roleName',
      component: 'Input',
      label: '名称',
      rules: [{ required: true, type: 'string' }],
      colProps: {
        span: 12,
      },
    },
    {
      field: 'description',
      component: 'InputTextArea',
      label: '描述',
    },
    {
      field: 'menus',
      component: 'Tree',
      label: '菜单权限',
      colProps: {
        span: 24,
      },
      componentProps: {
        checkable: true,
        vModelKey: 'checkedKeys',
        style: {
          height: '300px',
          paddingTop: '5px',
          overflow: 'auto',
          borderRadius: '6px',
          border: '1px solid #dcdfe6',
        },
      },
    },
  ]

  onMounted(async () => {
    columns.value.push({
      title: '测试列',
      dataIndex: 'isDeleted',
      align: 'center',
      hideInSearch: true,
      width: 100,
    })
    await nextTick()
    console.log('Columns:', columns) // 调试信息
  })
</script>
