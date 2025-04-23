import type { TableColumn } from '@/components/core/dynamic-table'
import { Tag } from 'ant-design-vue'
import { PublishStatusEnum, getEmunIndex, transformEnumToOptions } from '@/enums/commonEnum'

export const baseColumns: TableColumn[] = [
  {
    title: 'id',
    dataIndex: 'id',
    width: 60,
    sorter: true,
    hideInTable: true,
    hideInSearch: true,
  },
  {
    title: '名称',
    width: 150,
    dataIndex: 'name',
  },
  {
    title: '类型',
    dataIndex: 'typeName',
    hideInSearch: true,
  },
  {
    title: '状态',
    dataIndex: 'status',
    formItemProps: {
      component: 'Select',
      componentProps: {
        options: transformEnumToOptions(PublishStatusEnum),
      },
    },
    customRender: ({ text }) => (
      <Tag color={['error', 'success'][getEmunIndex(PublishStatusEnum, text)]}>
        {PublishStatusEnum[text]}
      </Tag>
    ),
  },
  {
    title: '描述',
    dataIndex: 'description',
    hideInSearch: true,
  },
  {
    title: '创建人',
    dataIndex: 'createUser',
    width: 160,
    hideInSearch: true,
  },
  {
    title: '创建时间',
    dataIndex: 'createTime',
    width: 160,
    sorter: true,
    hideInSearch: true,
  },
  {
    title: '更新人',
    dataIndex: 'updateUser',
    width: 160,
    hideInSearch: true,
    hideInTable: true,
  },
  {
    title: '更新时间',
    dataIndex: 'updateTime',
    width: 160,
    hideInSearch: true,
  },
]
