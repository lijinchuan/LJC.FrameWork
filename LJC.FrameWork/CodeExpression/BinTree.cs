using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LJC.FrameWork.CodeExpression
{
    [Serializable]
    public class BinTree<T>
    {
        /// <summary>
        /// 父节点
        /// </summary>
        public BinTree<T> FatherNode
        {
            get;
            set;
        }
        /// <summary>
        /// 节点
        /// </summary>
        public T TreeNode
        {
            get;
            set;
        }

        private BinTree<T> _leftTree;
        /// <summary>
        /// 左树
        /// </summary>
        public BinTree<T> LeftTree
        {
            get
            {
                return _leftTree;
            }
            set
            {
                _leftTree = value;
                value.FatherNode = this;
            }
        }

        private BinTree<T> _rightTree;
        /// <summary>
        /// 右树
        /// </summary>
        public BinTree<T> RightTree
        {
            get
            {
                return _rightTree;
            }
            set
            {
                _rightTree = value;
                value.FatherNode = this;
            }
        }


        public BinTree(T nodeValue)
        {
            TreeNode = nodeValue;
        }

        public BinTree()
        {

        }

        //预处理，执行以下操作,将后序遍历结果存储
        public BinTree<T> Pepare()
        {
            listBackForeach = BackForeach().ToArray();
            return this;
        }

        private IExpressPart[] listBackForeach = null;
        /// <summary>
        /// 后序遍历
        /// </summary>
        public IEnumerable<IExpressPart> BackForeach()
        {
            if (listBackForeach != null)
            {
                int len = listBackForeach.Length;
                for (int i = 0; i < len; i++)
                {
                    yield return listBackForeach[i];
                }
                yield break;
            }

            if (this.RightTree != null)
            {
                foreach (var node in this.RightTree.BackForeach())
                {
                    yield return node;
                }
                //RightTree.BackForeach();
            }
            if (this.LeftTree != null)
            {
                foreach (var node in this.LeftTree.BackForeach())
                {
                    yield return node;
                }
                //LeftTree.BackForeach();
            }

            IExpressPart o1 = (IExpressPart)this.TreeNode;
            if (o1 is CalExpress)
            {
                //Console.WriteLine((o1 as CalExpress).Express);
                yield return o1;
            }
            else if (o1 is CalSign)
            {
                //Console.WriteLine((o1 as CalSign).SignName);
                yield return o1;
            }
        }

        /// <summary>
        /// 向右节点深度搜索节点
        /// </summary>
        /// <param name="deep">搜索深度，从0开始</param>
        /// <returns></returns>
        public BinTree<T> RightDeepSearchNode(int deep)
        {
            if (deep <= 0)
                return this;

            int _lev = 1;
            BinTree<T> result = this.RightTree;
            while (_lev < deep && result.RightTree != null)
            {
                result = result.RightTree;
                _lev++;
            }
            if (_lev > deep)
                return null;

            return result;
        }

        /// <summary>
        /// 向上搜索父节点
        /// </summary>
        /// <param name="upDeep">向上搜索深度</param>
        /// <returns></returns>
        public BinTree<T> RightUpSearchParentNode(int upDeep)
        {
            if (upDeep <= 0 || this.FatherNode == null)
                return this;

            
            BinTree <T> result= this.FatherNode??this;
            int _deep = upDeep - 1;
            while (_deep > 0 && result.FatherNode != null)
            {
                result = result.FatherNode;
                _deep--;
            }
            //if (_deep > 0)
            //    return null;

            return result;
        }
    }
}
