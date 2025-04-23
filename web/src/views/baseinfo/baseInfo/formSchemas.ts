import type { FormSchema } from '@/components/core/schema-form'
import dicAction from '@/api/dev/dataDic'

/**
 * 表单信息
 */
export const formSchemas: FormSchema[] = [
  {
    field: 'name',
    component: 'Input',
    label: '名称',
    required: true,
    colProps: {
      span: 24,
    },
  },
  {
    field: 'content',
    component: 'Input',
    label: '内容',
    vIf: () => false,
  },
  {
    field: 'type',
    component: 'Select',
    label: '类型',
    required: true,
    // componentProps: {
    //   request: async () => await dicAction.getForSelect('工艺路线类型'),
    // },
  },
  {
    field: 'description',
    component: 'InputTextArea',
    label: '描述',
  },
]
